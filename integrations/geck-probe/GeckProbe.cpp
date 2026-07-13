#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <wincrypt.h>

#include <array>
#include <cstdio>
#include <string>

#include "common/ITypes.h"
#include "nvse/nvse_version.h"
#include "nvse/PluginAPI.h"

#ifndef WF_GECK_PROBE_LAUNCH_AUTHORIZED
#define WF_GECK_PROBE_LAUNCH_AUTHORIZED 0
#endif

#ifndef WF_GECK_PROBE_RUN_ID
#define WF_GECK_PROBE_RUN_ID "build-only-unapproved"
#endif

namespace
{
constexpr const char* kProbeName = "WastelandForge.GeckProbe";
constexpr const char* kProbeVersion = "0.1.0";
constexpr UInt32 kProbeVersionNumber = 1;
constexpr bool kLaunchAuthorized = WF_GECK_PROBE_LAUNCH_AUTHORIZED == 1;

HMODULE g_module = nullptr;

std::string ToHex(const BYTE* bytes, DWORD length)
{
    static constexpr char digits[] = "0123456789abcdef";
    std::string result(length * 2, '0');
    for (DWORD index = 0; index < length; ++index)
    {
        result[index * 2] = digits[(bytes[index] >> 4) & 0x0F];
        result[index * 2 + 1] = digits[bytes[index] & 0x0F];
    }

    return result;
}

bool GetProbeSha256(std::string& sha256)
{
    std::array<wchar_t, 32768> modulePath{};
    const DWORD pathLength = GetModuleFileNameW(g_module, modulePath.data(), static_cast<DWORD>(modulePath.size()));
    if (pathLength == 0 || pathLength >= modulePath.size())
    {
        return false;
    }

    HANDLE file = CreateFileW(
        modulePath.data(),
        GENERIC_READ,
        FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
        nullptr,
        OPEN_EXISTING,
        FILE_ATTRIBUTE_NORMAL,
        nullptr);
    if (file == INVALID_HANDLE_VALUE)
    {
        return false;
    }

    HCRYPTPROV provider = 0;
    HCRYPTHASH hash = 0;
    bool succeeded = false;
    if (CryptAcquireContextW(&provider, nullptr, nullptr, PROV_RSA_AES, CRYPT_VERIFYCONTEXT) &&
        CryptCreateHash(provider, CALG_SHA_256, 0, 0, &hash))
    {
        std::array<BYTE, 65536> buffer{};
        DWORD bytesRead = 0;
        succeeded = true;
        while (true)
        {
            if (!ReadFile(file, buffer.data(), static_cast<DWORD>(buffer.size()), &bytesRead, nullptr))
            {
                succeeded = false;
                break;
            }
            if (bytesRead == 0)
            {
                break;
            }
            if (!CryptHashData(hash, buffer.data(), bytesRead, 0))
            {
                succeeded = false;
                break;
            }
        }

        std::array<BYTE, 32> digest{};
        DWORD digestLength = static_cast<DWORD>(digest.size());
        if (succeeded && CryptGetHashParam(hash, HP_HASHVAL, digest.data(), &digestLength, 0) &&
            digestLength == digest.size())
        {
            sha256 = ToHex(digest.data(), digestLength);
        }
        else
        {
            succeeded = false;
        }
    }

    if (hash != 0)
    {
        CryptDestroyHash(hash);
    }
    if (provider != 0)
    {
        CryptReleaseContext(provider, 0);
    }
    CloseHandle(file);
    return succeeded;
}

bool EnsureDirectory(const std::wstring& path)
{
    if (CreateDirectoryW(path.c_str(), nullptr))
    {
        return true;
    }

    return GetLastError() == ERROR_ALREADY_EXISTS;
}

bool GetObservationRoot(std::wstring& root)
{
    const DWORD required = GetEnvironmentVariableW(L"LOCALAPPDATA", nullptr, 0);
    if (required <= 1)
    {
        return false;
    }

    std::wstring localAppData(required, L'\0');
    const DWORD written = GetEnvironmentVariableW(L"LOCALAPPDATA", localAppData.data(), required);
    if (written == 0 || written >= required)
    {
        return false;
    }
    localAppData.resize(written);

    const std::wstring forgeRoot = localAppData + L"\\WastelandForge";
    root = forgeRoot + L"\\GeckProbe";
    return EnsureDirectory(forgeRoot) && EnsureDirectory(root);
}

std::string GetUtcTimestamp()
{
    SYSTEMTIME utc{};
    GetSystemTime(&utc);
    std::array<char, 32> timestamp{};
    sprintf_s(
        timestamp.data(),
        timestamp.size(),
        "%04u-%02u-%02uT%02u:%02u:%02u.%03uZ",
        utc.wYear,
        utc.wMonth,
        utc.wDay,
        utc.wHour,
        utc.wMinute,
        utc.wSecond,
        utc.wMilliseconds);
    return timestamp.data();
}

std::wstring GetObservationFileName(const char* phase)
{
    std::wstring runId;
    for (const char value : std::string(WF_GECK_PROBE_RUN_ID))
    {
        runId.push_back(static_cast<wchar_t>(static_cast<unsigned char>(value)));
    }

    std::wstring phaseName;
    for (const char value : std::string(phase))
    {
        phaseName.push_back(static_cast<wchar_t>(static_cast<unsigned char>(value)));
    }

    return runId + L"-" + std::to_wstring(GetCurrentProcessId()) + L"-" + phaseName + L".json";
}

bool WriteAll(HANDLE file, const std::string& content)
{
    size_t offset = 0;
    while (offset < content.size())
    {
        const DWORD remaining = static_cast<DWORD>(content.size() - offset);
        DWORD written = 0;
        if (!WriteFile(file, content.data() + offset, remaining, &written, nullptr) || written == 0)
        {
            return false;
        }
        offset += written;
    }

    return FlushFileBuffers(file) != FALSE;
}

bool WriteObservation(const NVSEInterface* nvse, const char* phase, const char* result)
{
    std::wstring observationRoot;
    std::string probeSha256;
    if (!GetObservationRoot(observationRoot) || !GetProbeSha256(probeSha256))
    {
        return false;
    }

    std::array<char, 4096> json{};
    const int length = sprintf_s(
        json.data(),
        json.size(),
        "{\n"
        "  \"formatVersion\": \"0.1\",\n"
        "  \"kind\": \"wastelandforge.geck-host-probe-observation\",\n"
        "  \"probeName\": \"%s\",\n"
        "  \"probeVersion\": \"%s\",\n"
        "  \"probeSha256\": \"%s\",\n"
        "  \"approvedProbeRunId\": \"%s\",\n"
        "  \"launchAuthorized\": %s,\n"
        "  \"phase\": \"%s\",\n"
        "  \"result\": \"%s\",\n"
        "  \"nvseVersion\": \"0x%08lX\",\n"
        "  \"editorVersion\": \"0x%08lX\",\n"
        "  \"isEditor\": %s,\n"
        "  \"processId\": %lu,\n"
        "  \"utc\": \"%s\",\n"
        "  \"recordApisUsed\": false,\n"
        "  \"pluginMutation\": false\n"
        "}\n",
        kProbeName,
        kProbeVersion,
        probeSha256.c_str(),
        WF_GECK_PROBE_RUN_ID,
        kLaunchAuthorized ? "true" : "false",
        phase,
        result,
        nvse == nullptr ? 0 : nvse->nvseVersion,
        nvse == nullptr ? 0 : nvse->editorVersion,
        nvse != nullptr && nvse->isEditor != 0 ? "true" : "false",
        GetCurrentProcessId(),
        GetUtcTimestamp().c_str());
    if (length <= 0 || static_cast<size_t>(length) >= json.size())
    {
        return false;
    }

    const std::wstring finalPath = observationRoot + L"\\" + GetObservationFileName(phase);
    const std::wstring temporaryPath = finalPath + L".tmp";
    HANDLE file = CreateFileW(
        temporaryPath.c_str(),
        GENERIC_WRITE,
        0,
        nullptr,
        CREATE_ALWAYS,
        FILE_ATTRIBUTE_NORMAL,
        nullptr);
    if (file == INVALID_HANDLE_VALUE)
    {
        return false;
    }

    const bool written = WriteAll(file, std::string(json.data(), static_cast<size_t>(length)));
    CloseHandle(file);
    if (!written)
    {
        DeleteFileW(temporaryPath.c_str());
        return false;
    }

    if (!MoveFileExW(temporaryPath.c_str(), finalPath.c_str(), MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH))
    {
        DeleteFileW(temporaryPath.c_str());
        return false;
    }

    return true;
}

const char* GetRefusal(const NVSEInterface* nvse)
{
    if (nvse == nullptr)
    {
        return "refused-missing-interface";
    }
    if (nvse->isEditor == 0)
    {
        return "refused-runtime-host";
    }
    if (nvse->nvseVersion != PACKED_NVSE_VERSION)
    {
        return "refused-xnvse-version";
    }
    if (nvse->editorVersion < CS_VERSION_1_4_0_518)
    {
        return "refused-editor-version";
    }
    if (!kLaunchAuthorized)
    {
        return "refused-launch-not-authorized";
    }

    return nullptr;
}
}

BOOL WINAPI DllMain(HINSTANCE instance, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH)
    {
        g_module = instance;
        DisableThreadLibraryCalls(instance);
    }
    return TRUE;
}

extern "C" __declspec(dllexport) bool NVSEPlugin_Query(const NVSEInterface* nvse, PluginInfo* info)
{
    if (info == nullptr)
    {
        return false;
    }

    info->infoVersion = PluginInfo::kInfoVersion;
    info->name = kProbeName;
    info->version = kProbeVersionNumber;

    const char* refusal = GetRefusal(nvse);
    const char* result = refusal == nullptr ? "query-accepted" : refusal;
    const bool observed = WriteObservation(nvse, "query", result);
    return refusal == nullptr && observed;
}

extern "C" __declspec(dllexport) bool NVSEPlugin_Load(const NVSEInterface* nvse)
{
    const char* refusal = GetRefusal(nvse);
    const char* result = refusal == nullptr ? "load-accepted" : refusal;
    const bool observed = WriteObservation(nvse, "load", result);
    return refusal == nullptr && observed;
}
