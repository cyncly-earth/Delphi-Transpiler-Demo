unit classPerson;
 
interface
 
type
  TPerson = class
  private
    cID     : Integer;
    cClient : Integer;
    cFirst  : String;
    cLast   : String;
    cNotes  : String;
  public
    property PersonID : Integer read cID write cID;
    property Client   : Integer read cClient write cClient;
    property First    : String  read cFirst write cFirst;
    property Last     : String  read cLast write cLast;
    property Notes    : String  read cNotes write cNotes;
 
    constructor Create(
      nPersonID : Integer;
      nClient   : Integer;
      nLast     : String;
      nFirst    : String;
      nNotes    : String
    );
 
    function ToString : String;
  end;
 
implementation
 
constructor TPerson.Create(
  nPersonID : Integer;
  nClient   : Integer;
  nLast     : String;
  nFirst    : String;
  nNotes    : String);
begin
  cID     := nPersonID;
  cClient := nClient;
  cLast   := nLast;
  cFirst  := nFirst;
  cNotes  := nNotes;
end;
 
function TPerson.ToString : String;
begin
  Result := cFirst + ' ' + cLast;
  if cNotes <> '' then
    Result := Result + ' - ' + cNotes;
end;
 
end.
