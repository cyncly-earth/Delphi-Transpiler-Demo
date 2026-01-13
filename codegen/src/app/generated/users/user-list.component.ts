import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { UserService } from './user.service';
import { User } from './user.model';

@Component({
  selector: 'user-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent {
  private svc = inject(UserService);
  rows = signal<User[]>([]);
  loading = signal<boolean>(false);
  displayedColumns = ['id', 'firstName', 'email', 'isActive'];

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

  deleteRow(row: User) {
    if (!confirm('Delete this record?')) return;
    const id = row['id'];
    this.svc.delete(id).subscribe(() => this.refresh());
  }
}
