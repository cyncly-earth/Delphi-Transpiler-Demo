import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-list.component.html'
})
export class Order-listComponent {
  items = [
    { id: '', name: '', email: '', phoneNumber: '', internalNote: '' }
  ];
  columns = ['id', 'name', 'email', 'phoneNumber', 'internalNote'];
}