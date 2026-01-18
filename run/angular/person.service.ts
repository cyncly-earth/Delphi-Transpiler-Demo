import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Person } from './person.model';
@Injectable({ providedIn: 'root' })
export class PersonService {
  constructor(private http: HttpClient) {}
  addPerson(person: Person) {
    return this.http.post('/api/AddPerson', person);
  }
}
