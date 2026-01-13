using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AngularGenerator
{
    public void Generate(ScreenConfig config)
    {
        GenerateComponent(config);
        GenerateService(config);
        GenerateHtml(config);
    }

    private void GenerateComponent(ScreenConfig config)
    {
        var ts = $@"
import {{ Component }} from '@angular/core';
import {{ {config.Screen}Service }} from './{config.Screen.ToLower()}.service';

@Component({{
  selector: 'app-{config.Screen.ToLower()}',
  templateUrl: './{config.Screen.ToLower()}.component.html'
}})
export class {config.Screen}Component {{

  constructor(private service: {config.Screen}Service) {{ }}

  {config.Action.Name}() {{
    this.service.{config.Action.Calls}();
  }}
}}
";
        File.WriteAllText($"output/{config.Screen.ToLower()}.component.ts", ts);
    }

    private void GenerateService(ScreenConfig config)
    {
        var ts = $@"
import {{ Injectable }} from '@angular/core';
import {{ HttpClient }} from '@angular/common/http';

@Injectable({{ providedIn: 'root' }})
export class {config.Screen}Service {{

  constructor(private http: HttpClient) {{ }}

  {config.Action.Calls}() {{
    return this.http.post('/api/{config.Screen.ToLower()}', {{}});
  }}
}}
";
        File.WriteAllText($"output/{config.Screen.ToLower()}.service.ts", ts);
    }

    private void GenerateHtml(ScreenConfig config)
    {
        var fields = string.Join("\n",
            config.Fields.Select(f => $"<input placeholder=\"{f}\" />"));

        var html = $@"
<h2>{config.Screen}</h2>

{fields}

<button (click)=""{config.Action.Name}()"">
  {config.Action.Name}
</button>
";
        File.WriteAllText($"output/{config.Screen.ToLower()}.component.html", html);
    }
}
