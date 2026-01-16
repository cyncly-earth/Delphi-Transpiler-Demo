unit PersonView;
 
interface
 
uses
  Vcl.Forms, Vcl.StdCtrls, classPerson;
 
type
  TfrmPerson = class(TForm)
    btnAddPerson: TButton;
    edtFirst    : TEdit;
    edtLast     : TEdit;
    edtNotes    : TEdit;
    procedure btnAddPersonClick(Sender: TObject);
  end;
 
implementation
 
uses PersonController, SysUtils;
 
procedure TfrmPerson.btnAddPersonClick(Sender: TObject);
var
  Person : TPerson;
begin
  Person := TPerson.Create(
    0,                  // PersonID (0 for new)
    1,                  // ClientID
    edtLast.Text,
    edtFirst.Text,
    edtNotes.Text
  );
 
  try
    AddPerson(Person);
    ShowMessage('Person added successfully');
  finally
    Person.Free;
  end;
end;
 
end.
