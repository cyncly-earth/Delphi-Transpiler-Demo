import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { UserService } from './user.service';
import { User } from './user.model';

@Component({
  selector: 'user-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.scss']
})
export class UserFormComponent {
  private svc = inject(UserService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  id: string | null = null;
  loading = signal(false);
  form = new FormGroup({
      firstName: new FormControl('', [Validators.required]),
      email: new FormControl('', [Validators.email]),
      isActive: new FormControl(true)
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
    const value = this.form.value as User;
    this.loading.set(true);
    const req$ = !this.id || this.id === 'new'
      ? this.svc.create(value)
      : this.svc.update(this.id!, value);

    req$.subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/users']);
      },
      error: () => this.loading.set(false)
    });
  }
}
