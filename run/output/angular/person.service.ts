import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class PersonService {
  constructor(private http: HttpClient) {}

  addPerson(data?: any) {
    return this.http.post('/api/person', data);
  }

  editPerson(data?: any) {
    return this.http.put('/api/person', data);
  }

  deletePerson(data?: any) {
    return this.http.delete('/api/person', data);
  }

}
