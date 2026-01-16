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
