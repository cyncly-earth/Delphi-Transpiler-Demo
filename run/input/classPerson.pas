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

 
unit PersonController;
 
interface
 
uses classPerson;
 
procedure AddPerson(Person : TPerson);
 
implementation
 
uses ClientModuleUnit1;
 
procedure AddPerson(Person : TPerson);

begin

  with Module.mtPerson do

  begin

    if not Active then Open;
 
    Append;

    FieldByName('PersonID').Value := Person.PersonID;

    FieldByName('ClientID').Value := Person.Client;

    FieldByName('LastName').Value := Person.Last;

    FieldByName('FirstName').Value:= Person.First;

    FieldByName('Notes').Value    := Person.Notes;

    Post;

  end;

end;
 
end.

 