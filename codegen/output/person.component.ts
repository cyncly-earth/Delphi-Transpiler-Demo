
import { Component } from '@angular/core';
import { PersonService } from './person.service';

@Component({
  selector: 'app-person',
  templateUrl: './person.component.html'
})
export class PersonComponent {

  constructor(private service: PersonService) { }

  savePerson() {
    this.service.save();
  }
}
