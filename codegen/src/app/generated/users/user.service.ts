import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { User } from './user.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private baseUrl = '/api/users';

  list(): Observable<User[]> {
    return this.http.get<User[]>(`${this.baseUrl}/`);
  }

  get(id: number | string): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/${id}`);
  }

  create(data: User): Observable<User> {
    return this.http.post<User>(`${this.baseUrl}/`, data);
  }

  update(id: number | string, data: Partial<User>): Observable<User> {
    return this.http.put<User>(`${this.baseUrl}/${id}`, data);
  }

  delete(id: number | string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
