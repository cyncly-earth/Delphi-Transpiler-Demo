import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

/** ------------------ Utilities ------------------ */
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const toKebab = (s) =>
  s.replace(/([a-z0-9])([A-Z])/g, '$1-$2').replace(/\s+/g, '-').toLowerCase();
const toPascal = (s) =>
  s
    .replace(/(^\w|-\w)/g, (m) => m.replace('-', '').toUpperCase())
    .replace(/\s+/g, '');
const toCamel = (s) => s.charAt(0).toLowerCase() + s.slice(1);

const tsTypeMap = {
  string: 'string',
  number: 'number',
  boolean: 'boolean',
  date: 'string' // store as ISO string in model; map to Date if you prefer
};

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function writeFileSafe(filepath, content) {
  ensureDir(path.dirname(filepath));
  fs.writeFileSync(filepath, content, 'utf-8');
  console.log('✔ Created', filepath);
}

/** ------------------ Templating ------------------ */

function modelTemplate(entity) {
  const pascal = toPascal(entity.name);
  const lines = entity.fields.map((f) => {
    const t = tsTypeMap[f.type] || 'any';
    const optional = f.required ? '' : '?';
    return `  ${f.name}${optional}: ${t};`;
  });
  return `export interface ${pascal} {
${lines.join('\n')}
}
`;
}

function serviceTemplate(entity) {
  const pascal = toPascal(entity.name);
  const camel = toCamel(pascal);
  const kebab = toKebab(pascal);

  const baseUrl = entity.api?.baseUrl || `/api/${toKebab(entity.route || entity.name)}`;

  return `import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ${pascal} } from './${kebab}.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ${pascal}Service {
  private http = inject(HttpClient);
  private baseUrl = '${baseUrl}';

  list(): Observable<${pascal}[]> {
    return this.http.get<${pascal}[]>(\`\${this.baseUrl}/\`);
  }

  get(id: number | string): Observable<${pascal}> {
    return this.http.get<${pascal}>(\`\${this.baseUrl}/\${id}\`);
  }

  create(data: ${pascal}): Observable<${pascal}> {
    return this.http.post<${pascal}>(\`\${this.baseUrl}/\`, data);
  }

  update(id: number | string, data: Partial<${pascal}>): Observable<${pascal}> {
    return this.http.put<${pascal}>(\`\${this.baseUrl}/\${id}\`, data);
  }

  delete(id: number | string): Observable<void> {
    return this.http.delete<void>(\`\${this.baseUrl}/\${id}\`);
  }
}
`;
}

function listComponentTemplateTS(entity) {
  const pascal = toPascal(entity.name);
  const kebab = toKebab(pascal);
  const camel = toCamel(pascal);
  const primaryKey = entity.fields.find(f => f.name === 'id')?.name || 'id';

  const columns = entity.fields.slice(0, 5).map(f => `'${f.name}'`).join(', ');
  return `import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ${pascal}Service } from './${kebab}.service';
import { ${pascal} } from './${kebab}.model';

@Component({
  selector: '${kebab}-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './${kebab}-list.component.html',
  styleUrls: ['./${kebab}-list.component.${entity.ui?.style === 'css' ? 'css' : 'scss'}']
})
export class ${pascal}ListComponent {
  private svc = inject(${pascal}Service);
  rows = signal<${pascal}[]>([]);
  loading = signal<boolean>(false);
  displayedColumns = [${columns}];

  ngOnInit() {
    this.refresh();
  }

  refresh() {
    this.loading.set(true);
    this.svc.list().subscribe({
      next: (data) => { this.rows.set(data); this.loading.set(false); },
      error: () => { this.loading.set(false); }
    });
  }

  deleteRow(row: ${pascal}) {
    if (!confirm('Delete this record?')) return;
    const id = row['${primaryKey}'];
    this.svc.delete(id).subscribe(() => this.refresh());
  }
}
`;
}

function listComponentTemplateHTML(entity) {
  const pascal = toPascal(entity.name);
  const kebab = toKebab(pascal);
  const primaryKey = entity.fields.find(f => f.name === 'id')?.name || 'id';

  const headers = entity.fields.slice(0, 5).map(f => `<th>${f.name}</th>`).join('\n      ');
  const cells = entity.fields.slice(0, 5).map(f => `<td>{{ r.${f.name} }}</td>`).join('\n        ');

  return `<div class="list">
  <h2>${pascal} List</h2>
  <a class="btn" [routerLink]="['/${entity.route}', 'new']">➕ New</a>

  <table *ngIf="!loading() && rows().length; else loadingTpl">
    <thead>
      <tr>
      ${headers}
        <th></th>
      </tr>
    </thead>
    <tbody>
      <tr *ngFor="let r of rows()">
        ${cells}
        <td class="actions">
          <a [routerLink]="['/${entity.route}', r.${primaryKey}]">✏️ Edit</a>
          <button (click)="deleteRow(r)">🗑 Delete</button>
        </td>
      </tr>
    </tbody>
  </table>

  <ng-template #loadingTpl>
    <p *ngIf="loading()">Loading…</p>
    <p *ngIf="!loading()">No data</p>
  </ng-template>
</div>
`;
}

function listComponentStyle(entity) {
  return `.list { padding: 16px; }
table { width: 100%; border-collapse: collapse; }
th, td { border-bottom: 1px solid #e0e0e0; padding: 8px; text-align: left; }
.actions { white-space: nowrap; }
.btn { display: inline-block; margin: 0 0 12px; border: 1px solid #1976d2; color: #1976d2; padding: 6px 10px; border-radius: 4px; text-decoration: none; }
`;
}

function formComponentTemplateTS(entity) {
  const pascal = toPascal(entity.name);
  const kebab = toKebab(pascal);
  const camel = toCamel(pascal);
  const primaryKey = entity.fields.find(f => f.name === 'id')?.name || 'id';

  // Build form controls
  const controls = entity.fields
    .filter(f => !f.readonly || f.name !== primaryKey)
    .map(f => {
      const validators = [];
      if (f.required) validators.push('Validators.required');
      if (f.validators?.includes('email')) validators.push('Validators.email');
      const v = validators.length ? `, [${validators.join(', ')}]` : '';
      const defVal =
        f.default !== undefined
          ? JSON.stringify(f.default)
          : f.type === 'boolean'
            ? 'false'
            : f.type === 'number'
              ? 'null'
              : "''";
      return `      ${f.name}: new FormControl(${defVal}${v})`;
    })
    .join(',\n');

  return `import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ${pascal}Service } from './${kebab}.service';
import { ${pascal} } from './${kebab}.model';

@Component({
  selector: '${kebab}-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './${kebab}-form.component.html',
  styleUrls: ['./${kebab}-form.component.${entity.ui?.style === 'css' ? 'css' : 'scss'}']
})
export class ${pascal}FormComponent {
  private svc = inject(${pascal}Service);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  id: string | null = null;
  loading = signal(false);
  form = new FormGroup({
${controls}
  });

  ngOnInit() {
    this.id = this.route.snapshot.paramMap.get('id');
    if (this.id && this.id !== 'new') {
      this.loading.set(true);
      this.svc.get(this.id).subscribe({
        next: (data) => {
          this.form.patchValue(data as any);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
    }
  }

  save() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.value as ${pascal};
    this.loading.set(true);
    const req$ = !this.id || this.id === 'new'
      ? this.svc.create(value)
      : this.svc.update(this.id!, value);

    req$.subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/${entity.route}']);
      },
      error: () => this.loading.set(false)
    });
  }
}
`;
}

function formComponentTemplateHTML(entity) {
  const pascal = toPascal(entity.name);
  const controls = entity.fields
    .filter(f => f.name !== 'id' || !f.readonly) // hide readonly id
    .map(f => {
      const label = f.name;
      const control = (() => {
        switch (f.type) {
          case 'boolean':
            return `<input type="checkbox" [formControlName]="'${f.name}'" />`;
          case 'number':
            return `<input type="number" class="control" [formControlName]="'${f.name}'" />`;
          case 'date':
            return `<input type="date" class="control" [formControlName]="'${f.name}'" />`;
          default:
            return `<input type="text" class="control" [formControlName]="'${f.name}'" />`;
        }
      })();
      const requiredMsg = f.required
        ? `\n      <div class="error" *ngIf="form.get('${f.name}')?.touched && form.get('${f.name}')?.hasError('required')">Required</div>`
        : '';
      const emailMsg = f.validators?.includes('email')
        ? `\n      <div class="error" *ngIf="form.get('${f.name}')?.touched && form.get('${f.name}')?.hasError('email')">Invalid email</div>`
        : '';
      return `<div class="field">
    <label>${label}</label>
    ${control}
    ${requiredMsg}${emailMsg}
  </div>`;
    })
    .join('\n\n  ');

  return `<div class="form">
  <h2>${pascal} Form</h2>

  <form [formGroup]="form" (ngSubmit)="save()">
    ${controls}

    <div class="actions">
      <button type="submit" [disabled]="loading()">💾 Save</button>
      <a routerLink="/${entity.route}">Cancel</a>
    </div>
  </form>
</div>
`;
}

function formComponentStyle(entity) {
  return `.form { padding: 16px; max-width: 640px; }
.field { margin-bottom: 12px; display: flex; flex-direction: column; }
label { font-weight: 600; margin-bottom: 6px; }
.control { padding: 8px; border: 1px solid #ccc; border-radius: 4px; }
.error { color: #c62828; font-size: 12px; margin-top: 4px; }
.actions { display: flex; gap: 8px; margin-top: 16px; }
`;
}

function routesTemplate(entity) {
  const pascal = toPascal(entity.name);
  const kebab = toKebab(pascal);
  return `import { Routes } from '@angular/router';

export const ${toCamel(pascal)}Routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./${kebab}-list.component').then(m => m.${pascal}ListComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./${kebab}-form.component').then(m => m.${pascal}FormComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./${kebab}-form.component').then(m => m.${pascal}FormComponent)
  }
];
`;
}

/** ------------------ Main ------------------ */

function generate(specPath) {
  if (!specPath) {
    console.error('Usage: node generator.js <path-to-entity.json>');
    process.exit(1);
  }
  const json = JSON.parse(fs.readFileSync(specPath, 'utf-8'));
  const entity = {
    ui: { style: 'scss', standalone: true },
    ...json
  };

  const pascal = toPascal(entity.name);
  const kebab = toKebab(pascal);

  const outDir = entity.outputDir || path.join('src', 'app', 'generated', entity.route || toKebab(entity.name));
  ensureDir(outDir);

  // Files
  writeFileSafe(path.join(outDir, `${kebab}.model.ts`), modelTemplate(entity));
  writeFileSafe(path.join(outDir, `${kebab}.service.ts`), serviceTemplate(entity));

  // List component
  writeFileSafe(path.join(outDir, `${kebab}-list.component.ts`), listComponentTemplateTS(entity));
  writeFileSafe(path.join(outDir, `${kebab}-list.component.html`), listComponentTemplateHTML(entity));
  writeFileSafe(
    path.join(outDir, `${kebab}-list.component.${entity.ui?.style === 'css' ? 'css' : 'scss'}`),
    listComponentStyle(entity)
  );

  // Form component
  writeFileSafe(path.join(outDir, `${kebab}-form.component.ts`), formComponentTemplateTS(entity));
  writeFileSafe(path.join(outDir, `${kebab}-form.component.html`), formComponentTemplateHTML(entity));
  writeFileSafe(
    path.join(outDir, `${kebab}-form.component.${entity.ui?.style === 'css' ? 'css' : 'scss'}`),
    formComponentStyle(entity)
  );

  // Routes
  writeFileSafe(path.join(outDir, `routes.ts`), routesTemplate(entity));

  console.log('\nDone. Next steps printed below 👇');
  console.log(`\n1) Import HttpClientModule once in your app (if not already):
   import { provideHttpClient } from '@angular/common/http';

   bootstrapApplication(AppComponent, {
     providers: [provideHttpClient()]
   });

2) Add a parent route in your app.routes.ts:
   {
     path: '${entity.route || toKebab(entity.name)}',
     loadChildren: () => import('./${path.relative('src/app', outDir)}/routes').then(m => m.${toCamel(pascal)}Routes)
   }

3) Navigate to /${entity.route || toKebab(entity.name)} to see the list view.`);
}

generate(process.argv[2]);
