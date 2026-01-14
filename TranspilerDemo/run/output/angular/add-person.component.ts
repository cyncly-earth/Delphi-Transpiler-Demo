import { Component } from '@angular/core';
import { PersonService } from './person.service';
import { Person } from './person.model';

@Component({
  selector: 'app-add-person',
  templateUrl: './add-person.component.html'
})
export class AddPersonComponent {
  model: Person = {
    first: '',
    last: '',
    notes: '',
  };

  constructor(private service: PersonService) {}

  submit() {
    this.service.addPerson(this.model).subscribe();
  }
}
