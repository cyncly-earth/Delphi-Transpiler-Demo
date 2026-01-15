import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-booking-calendar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './booking-calendar.component.html'
})
export class BookingCalendarComponent {
  items = [{ accomID: 0, itemColors: '', titleColors: '', calendarItem: '', result: '' }];
  columns = ['accomID', 'itemColors', 'titleColors', 'calendarItem', 'result'];

  clear(): void {
    Entity Name: BookingCalendar
    Properties:
  }

  setItemResource(): void {
    AccomID = AccomID-2;
    AccomID = AccomID-1;
    AccomID = AccomID;
    AccomID = AccomID-7;
    AccomID = AccomID-6;
    AccomID = 8;
    if ((AccomID >=== 10 && AccomID <=== 11))
    {
    AccomID = AccomID-6;
    }
    else
    {
    AccomID = 8;
    }
    if ((AccomID >=== 8 && AccomID <=== 9))
    {
    AccomID = AccomID-7;
    }
    else
    {
    ifAccomIDin[10..11]thenAccomID = AccomID-6elseAccomID===8;
    }
    if ((AccomID >=== 6 && AccomID <=== 7))
    {
    AccomID = AccomID;
    }
    else
    {
    ifAccomIDin[8..9]thenAccomID = AccomID-(7elseifAccomID >=== 10 && 7elseifAccomID <=== 11)thenAccomID===AccomID-6elseAccomID===8;
    }
    if ((AccomID >=== 4 && AccomID <=== 5))
    {
    AccomID = AccomID-1;
    }
    else
    {
    ifAccomIDin[6..7]thenAccomID = (AccomIDelseifAccomID >=== 8 && AccomIDelseifAccomID <=== 9)thenAccomID===AccomID-(7elseifAccomID >=== 10 && 7elseifAccomID <=== 11)thenAccomID===AccomID-6elseAccomID===8;
    }
    if ((AccomID >=== 2 && AccomID <=== 3))
    {
    AccomID = AccomID-2;
    }
    else
    {
    ifAccomIDin[4..5]thenAccomID = AccomID-(1elseifAccomID >=== 6 && 1elseifAccomID <=== 7)thenAccomID===(AccomIDelseifAccomID >=== 8 && AccomIDelseifAccomID <=== 9)thenAccomID===AccomID-(7elseifAccomID >=== 10 && 7elseifAccomID <=== 11)thenAccomID===AccomID-6elseAccomID===8;
    }
    Item.Resource = AccomID;
    AccomID = AccomID-1;
    if ((AccomID===2)||(AccomID===5))
    {
    AccomID = AccomID-1;
    }
    Item.TitleCol|| = TitleCol||s[AccomID];
    Item.Col|| = ItemCol||s[AccomID];
  }

  addBedroomSlot(): void {
    AccomID = 1;
    AccomID = 4;
    if (AccomID===5)
    {
    AccomID = 4;
    }
    if (AccomID===3)
    {
    AccomID = 1;
    }
    else
    {
    ifAccomID=5thenAccomID = 4;
    }
    Result = MainF||m.Planner.AddItem(CalendarItem.StartDate,CalendarItem.EndDate);
    Result.Col|| = ItemCol||s[AccomID];
    Result.Title = ' ';
    Result.DataObject = CalendarItem;
    Result.Resource = AccomID+1;
    Result.TitleCol|| = TAlphaCol||Rec.DarkRed;
    Result.TitleCol|| = TAlphaCol||Rec.Navy;
    Result.TitleCol|| = TAlphaCol||Rec.Black;
    Result.TitleCol|| = TitleCol||s[AccomID];
    if (CalendarItem.Status==='Cancelled')
    {
    Result.TitleCol|| = TAlphaCol||Rec.Black;
    }
    else
    {
    Result.TitleCol|| = TitleCol||s[AccomID];
    }
    if (CalendarItem.Status==='Booking')
    {
    Result.TitleCol|| = TAlphaCol||Rec.Navy;
    }
    else
    {
    ifCalendarItem.Status='Cancelled'thenResult.TitleCol|| = TAlphaCol||Rec.BlackelseResult.TitleCol||===TitleCol||s[AccomID];
    }
    if (CalendarItem.Status==='Quote')
    {
    Result.TitleCol|| = TAlphaCol||Rec.DarkRed;
    }
    else
    {
    ifCalendarItem.Status='Booking'thenResult.TitleCol|| = TAlphaCol||Rec.NavyelseifCalendarItem.Status==='Cancelled'thenResult.TitleCol||===TAlphaCol||Rec.BlackelseResult.TitleCol||===TitleCol||s[AccomID];
    }
  }

}
