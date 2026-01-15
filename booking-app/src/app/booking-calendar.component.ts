import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface BookingItem {
  accomID: number;
  itemColors: string;
  titleColors: string;
  calendarItem: string;
  result: string;
}

@Component({
  selector: 'app-booking-calendar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './booking-calendar.component.html'
})
export class BookingCalendarComponent {
  items: BookingItem[] = [
    { 
      accomID: 1, 
      itemColors: 'red', 
      titleColors: 'blue', 
      calendarItem: 'Room A', 
      result: 'Booked' 
    },
    { 
      accomID: 2, 
      itemColors: 'blue', 
      titleColors: 'green', 
      calendarItem: 'Room B', 
      result: 'Available' 
    },
    { 
      accomID: 3, 
      itemColors: 'green', 
      titleColors: 'yellow', 
      calendarItem: 'Room C', 
      result: 'Pending' 
    }
  ];
  
  columns = ['accomID', 'itemColors', 'titleColors', 'calendarItem', 'result'];

  setItemResource(): void {
    console.log('setItemResource() called');
    // Logic to set item resource properties
    this.items.forEach(item => {
      item.itemColors = this.getColorForAccomID(item.accomID);
    });
  }

  addBedroomSlot(): void {
    console.log('addBedroomSlot() called');
    // Logic to add a new bedroom slot
    const newAccomID = Math.max(...this.items.map(i => i.accomID)) + 1;
    this.items.push({
      accomID: newAccomID,
      itemColors: this.getColorForAccomID(newAccomID),
      titleColors: 'purple',
      calendarItem: `Room ${String.fromCharCode(64 + newAccomID)}`,
      result: 'New'
    });
  }

  private getColorForAccomID(accomID: number): string {
    const colors = ['red', 'blue', 'green', 'yellow', 'purple', 'orange'];
    return colors[accomID % colors.length];
  }
}
