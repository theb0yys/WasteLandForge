unit UserScript;

var
  Evidence: TStringList;

const
  EvidencePath = 'D:\documents\GitHub\WasteLandForge\the forge\mods\Couriers-Emergency-Cache\evidence\geck-authoring\xedit-local-record-scan.tsv';

function Process(e: IInterface): Integer; forward;

procedure ScanTree(e: IInterface);
var
  i: Integer;
  child: IInterface;
begin
  for i := 0 to Pred(ElementCount(e)) do begin
    child := ElementByIndex(e, i);
    if ElementType(child) = etMainRecord then
      Process(child)
    else if ElementType(child) = etGroupRecord then
      ScanTree(child);
  end;
end;

function SafeValue(e: IInterface; path: string): string;
begin
  Result := StringReplace(GetElementEditValues(e, path), #9, ' ', [rfReplaceAll]);
  Result := StringReplace(Result, #13, ' ', [rfReplaceAll]);
  Result := StringReplace(Result, #10, ' ', [rfReplaceAll]);
end;

function ContainsRelevantText(value: string): Boolean;
var
  lowered: string;
begin
  lowered := LowerCase(value);
  Result :=
    (Pos('goodspring', lowered) > 0) or
    (Pos('docmitchell', lowered) > 0) or
    (Pos('doc mitchell', lowered) > 0);
end;

function ParentCell(e: IInterface): IInterface;
var
  group: IInterface;
begin
  Result := nil;
  group := GetContainer(e);
  while Assigned(group) and (ElementType(group) <> etGroupRecord) do
    group := GetContainer(group);
  if Assigned(group) then
    Result := ChildrenOf(group);
end;

procedure AddRecord(kind: string; e: IInterface; note: string);
begin
  Evidence.Add(
    kind + #9 +
    Signature(e) + #9 +
    IntToHex(FixedFormID(e), 8) + #9 +
    SafeValue(e, 'EDID') + #9 +
    SafeValue(e, 'FULL') + #9 +
    note
  );
end;

function Initialize: Integer;
var
  i: Integer;
begin
  Result := 0;
  Evidence := TStringList.Create;
  Evidence.Add('evidence_kind' + #9 + 'signature' + #9 + 'fixed_form_id' + #9 + 'editor_id' + #9 + 'full_name' + #9 + 'details');
  Evidence.Add('scan-metadata' + #9 + 'META' + #9 + '00000000' + #9 + 'WF_CouriersEmergencyCache_EvidenceScan' + #9 + '' + #9 + 'read-only xEdit record scan; expected loaded source FalloutNV.esm');
  for i := 0 to Pred(FileCount) do
    if SameText(Name(FileByIndex(i)), 'FalloutNV.esm') then
      ScanTree(FileByIndex(i));
end;

function Process(e: IInterface): Integer;
var
  sig, edid, fullName, baseEdid, baseName, details: string;
  base, cell: IInterface;
  fixedId: Cardinal;
begin
  Result := 0;
  if not SameText(GetFileName(e), 'FalloutNV.esm') then
    Exit;
  sig := Signature(e);
  fixedId := FixedFormID(e);
  edid := SafeValue(e, 'EDID');
  fullName := SafeValue(e, 'FULL');

  if (fixedId = $000151A3) or (fixedId = $0000000F) then begin
    AddRecord('required-item', e, 'exact requested inventory base record');
    Exit;
  end;

  if (sig = 'CELL') and (ContainsRelevantText(edid) or ContainsRelevantText(fullName)) then begin
    details := 'gridX=' + SafeValue(e, 'XCLC\X') + '; gridY=' + SafeValue(e, 'XCLC\Y');
    AddRecord('candidate-cell', e, details);
    Exit;
  end;

  if (sig = 'WRLD') and (ContainsRelevantText(edid) or ContainsRelevantText(fullName)) then begin
    AddRecord('candidate-worldspace', e, 'matching Goodsprings/Doc Mitchell text');
    Exit;
  end;

  if sig = 'CONT' then begin
    if (Pos('footlocker', LowerCase(edid + ' ' + fullName)) > 0) or
       (Pos('ammo box', LowerCase(edid + ' ' + fullName)) > 0) or
       (Pos('ammobox', LowerCase(edid + ' ' + fullName)) > 0) or
       (Pos('crate', LowerCase(edid + ' ' + fullName)) > 0) or
       (Pos('toolbox', LowerCase(edid + ' ' + fullName)) > 0) then
      AddRecord('candidate-container-base', e, 'candidate clone source; operator selection still required');
    Exit;
  end;

  if (sig <> 'REFR') and (sig <> 'ACHR') then
    Exit;

  base := BaseRecord(e);
  if Assigned(base) then begin
    baseEdid := EditorID(base);
    baseName := SafeValue(base, 'FULL');
  end;

  if ContainsRelevantText(edid + ' ' + fullName + ' ' + baseEdid + ' ' + baseName) then begin
    cell := ParentCell(e);
    details :=
      'base=' + IntToHex(FixedFormID(base), 8) + ':' + baseEdid +
      '; cell=' + IntToHex(FixedFormID(cell), 8) + ':' + EditorID(cell) +
      '; posX=' + SafeValue(e, 'DATA\Position\X') +
      '; posY=' + SafeValue(e, 'DATA\Position\Y') +
      '; posZ=' + SafeValue(e, 'DATA\Position\Z') +
      '; rotX=' + SafeValue(e, 'DATA\Rotation\X') +
      '; rotY=' + SafeValue(e, 'DATA\Rotation\Y') +
      '; rotZ=' + SafeValue(e, 'DATA\Rotation\Z');
    AddRecord('candidate-reference', e, details);
  end;
end;

function Finalize: Integer;
begin
  Result := 0;
  ForceDirectories(ExtractFilePath(EvidencePath));
  Evidence.SaveToFile(EvidencePath);
  AddMessage('WastelandForge evidence written: ' + EvidencePath);
  Evidence.Free;
end;

end.
