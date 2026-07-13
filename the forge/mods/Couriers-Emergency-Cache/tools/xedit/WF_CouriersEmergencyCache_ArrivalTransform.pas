unit UserScript;

const
  EvidencePath = 'D:\documents\GitHub\WasteLandForge\the forge\mods\Couriers-Emergency-Cache\evidence\geck-authoring\xedit-doc-mitchell-arrival.tsv';

function Initialize: Integer;
var
  i: Integer;
  sourceFile, interiorDoor, exteriorDoor, exteriorCell: IInterface;
  evidence: TStringList;
begin
  Result := 0;
  evidence := TStringList.Create;
  evidence.Add('evidence_kind' + #9 + 'interior_door_form_id' + #9 + 'exterior_door_form_id' + #9 + 'exterior_cell_form_id' + #9 + 'arrival_x' + #9 + 'arrival_y' + #9 + 'arrival_z' + #9 + 'arrival_rot_x' + #9 + 'arrival_rot_y' + #9 + 'arrival_rot_z');

  sourceFile := nil;
  for i := 0 to Pred(FileCount) do
    if SameText(GetFileName(FileByIndex(i)), 'FalloutNV.esm') then
      sourceFile := FileByIndex(i);
  if not Assigned(sourceFile) then begin
    evidence.Add('error' + #9 + 'FalloutNV.esm was not loaded');
  end else begin
    interiorDoor := RecordByFormID(sourceFile, $00103E61, False);
    exteriorDoor := WinningOverride(LinksTo(ElementByPath(interiorDoor, 'XTEL\Door')));
    exteriorCell := GetContainer(exteriorDoor);
    while Assigned(exteriorCell) and (ElementType(exteriorCell) <> etGroupRecord) do
      exteriorCell := GetContainer(exteriorCell);
    if Assigned(exteriorCell) then
      exteriorCell := ChildrenOf(exteriorCell);
    evidence.Add(
      'doc-mitchell-exterior-arrival' + #9 +
      IntToHex(FixedFormID(interiorDoor), 8) + #9 +
      IntToHex(FixedFormID(exteriorDoor), 8) + #9 +
      IntToHex(FixedFormID(exteriorCell), 8) + #9 +
      GetElementEditValues(interiorDoor, 'XTEL\Position\X') + #9 +
      GetElementEditValues(interiorDoor, 'XTEL\Position\Y') + #9 +
      GetElementEditValues(interiorDoor, 'XTEL\Position\Z') + #9 +
      GetElementEditValues(interiorDoor, 'XTEL\Rotation\X') + #9 +
      GetElementEditValues(interiorDoor, 'XTEL\Rotation\Y') + #9 +
      GetElementEditValues(interiorDoor, 'XTEL\Rotation\Z')
    );
  end;

  ForceDirectories(ExtractFilePath(EvidencePath));
  evidence.SaveToFile(EvidencePath);
  AddMessage('WastelandForge arrival evidence written: ' + EvidencePath);
  evidence.Free;
end;

end.
