unit PersonView;
interface
procedure btnAddClick;
implementation
procedure btnAddClick;
var
  Person : TPerson;
begin
  // UI Logic
  Person := TPerson.Create(edtFirst.Text, edtLast.Text);
  PersonController.AddPerson(Person);
end;
end.
