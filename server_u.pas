unit server_u;

interface

uses
  Winapi.Windows, Winapi.Messages, System.SysUtils, System.Variants, System.Classes, Vcl.Graphics,
  Vcl.Controls, Vcl.Forms, Vcl.Dialogs, ShellAPI;

type
  TForm1 = class(TForm)
    procedure FormCreate(Sender: TObject);
  private
    sServerScriptPath : string;
  public
    { Public declarations }
  end;

var
  Form1: TForm1;

implementation

{$R *.dfm}

procedure CallScript(const scriptPath : string);
begin
  ShellExecute(0, 'open', PChar(scriptPath), nil, nil, SW_SHOWNORMAL);
end;

procedure TForm1.FormCreate(Sender: TObject);
begin
  sServerScriptPath := 'serverScript\bin\Debug\net8.0\serverScript.exe';
  CallScript(sServerScriptPath);

end;
end.
