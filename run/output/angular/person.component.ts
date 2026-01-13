import { Component } from '@angular/core';
import { PersonService } from './person.service';

@Component({
  selector: 'app-person',
  templateUrl: './person.component.html'
})
export class PersonComponent {
  constructor(private service: PersonService) {}

  addPerson() {
    this.service.addPerson().subscribe();
  }

  editPerson() {
    this.service.editPerson().subscribe();
  }

  deletePerson() {
    this.service.deletePerson().subscribe();
  }

}
