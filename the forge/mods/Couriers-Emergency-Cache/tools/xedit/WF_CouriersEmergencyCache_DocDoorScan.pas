unit UserScript;

var
  Evidence: TStringList;
  ExteriorDoor: IInterface;
  ExteriorCellId: Cardinal;
  DoorX, DoorY: Extended;

const
  EvidencePath = 'D:\documents\GitHub\WasteLandForge\the forge\mods\Couriers-Emergency-Cache\evidence\geck-authoring\xedit-doc-mitchell-exterior.tsv';

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

function Clean(value: string): string;
begin
  Result := StringReplace(value, #9, ' ', [rfReplaceAll]);
end;

procedure AddReference(kind: string; e: IInterface);
var
  base, cell: IInterface;
begin
  base := BaseRecord(e);
  cell := ParentCell(e);
  Evidence.Add(
    kind + #9 +
    IntToHex(FixedFormID(e), 8) + #9 +
    Clean(EditorID(e)) + #9 +
    IntToHex(FixedFormID(base), 8) + #9 +
    Clean(EditorID(base)) + #9 +
    IntToHex(FixedFormID(cell), 8) + #9 +
    Clean(EditorID(cell)) + #9 +
    Clean(GetElementEditValues(e, 'DATA\Position\X')) + #9 +
    Clean(GetElementEditValues(e, 'DATA\Position\Y')) + #9 +
    Clean(GetElementEditValues(e, 'DATA\Position\Z')) + #9 +
    Clean(GetElementEditValues(e, 'DATA\Rotation\X')) + #9 +
    Clean(GetElementEditValues(e, 'DATA\Rotation\Y')) + #9 +
    Clean(GetElementEditValues(e, 'DATA\Rotation\Z'))
  );
end;

function Initialize: Integer;
var
  i: Integer;
  sourceFile, interiorDoor: IInterface;
begin
  Result := 0;
  Evidence := TStringList.Create;
  Evidence.Add('evidence_kind' + #9 + 'reference_form_id' + #9 + 'reference_editor_id' + #9 + 'base_form_id' + #9 + 'base_editor_id' + #9 + 'cell_form_id' + #9 + 'cell_editor_id' + #9 + 'pos_x' + #9 + 'pos_y' + #9 + 'pos_z' + #9 + 'rot_x' + #9 + 'rot_y' + #9 + 'rot_z');

  sourceFile := nil;
  for i := 0 to Pred(FileCount) do
    if SameText(Name(FileByIndex(i)), 'FalloutNV.esm') or SameText(GetFileName(FileByIndex(i)), 'FalloutNV.esm') then
      sourceFile := FileByIndex(i);
  if not Assigned(sourceFile) then begin
    Evidence.Add('error' + #9 + 'FalloutNV.esm was not loaded');
    Exit;
  end;

  interiorDoor := RecordByFormID(sourceFile, $00103E61, False);
  ExteriorDoor := WinningOverride(LinksTo(ElementByPath(interiorDoor, 'XTEL\Door')));
  if not Assigned(ExteriorDoor) then begin
    Evidence.Add('error' + #9 + 'Doc Mitchell exterior teleport door could not be resolved');
    Exit;
  end;

  ExteriorCellId := FixedFormID(ParentCell(ExteriorDoor));
  DoorX := GetElementNativeValues(ExteriorDoor, 'DATA\Position\X');
  DoorY := GetElementNativeValues(ExteriorDoor, 'DATA\Position\Y');
  AddReference('doc-mitchell-exterior-door', ExteriorDoor);
end;

function Process(e: IInterface): Integer;
var
  cell: IInterface;
  x, y: Extended;
begin
  Result := 0;
  if not Assigned(ExteriorDoor) then
    Exit;
  if not SameText(GetFileName(e), 'FalloutNV.esm') then
    Exit;
  if Signature(e) <> 'REFR' then
    Exit;
  x := GetElementNativeValues(e, 'DATA\Position\X');
  y := GetElementNativeValues(e, 'DATA\Position\Y');
  if (Abs(x - DoorX) <= 1500.0) and (Abs(y - DoorY) <= 1500.0) then
    AddReference('nearby-exterior-reference', e);
end;

function Finalize: Integer;
begin
  Result := 0;
  ForceDirectories(ExtractFilePath(EvidencePath));
  Evidence.SaveToFile(EvidencePath);
  AddMessage('WastelandForge Doc Mitchell evidence written: ' + EvidencePath);
  Evidence.Free;
end;

end.
