import { TimePeriod } from '../timePeriod.enum';

export class Helper {
  static parseDateOrDefault(dateStr, defaultValue) : Date {
      var parsedVal = Date.parse(dateStr);
      if(isNaN(parsedVal)) {
        return defaultValue;
      }
  
      return new Date(parsedVal);
    };

  // TODO: This is a bad approach. But for now...
  static parseTimePeriodByName(periodName: string) : TimePeriod {
    if(periodName === TimePeriod[TimePeriod.Quarter]) {
      return TimePeriod.Quarter;
    }
    if(periodName === TimePeriod[TimePeriod.Month]) {
      return TimePeriod.Month;
    }
    if(periodName === TimePeriod[TimePeriod.Week]) {
      return TimePeriod.Week;
    }
    if(periodName === TimePeriod[TimePeriod.Day]) {
      return TimePeriod.Day;
    }

    return null;
  };

  // TODO: This is a bad approach. But for now...
  static checkInputDecimal(event): boolean {
    const charCode = (event.which) ? event.which : event.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57) && charCode != 46) {
      return false;
    }
    if (charCode === 46 && event.target.value.indexOf('.') >= 0) {
      return false;
    }

    return true;
  }

  static checkInputInt(event): boolean {
    const charCode = (event.which) ? event.which : event.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
      return false;
    }

    return true;
  }

  static clearUtcTimeOfDate(date: Date) : Date {
    date.setUTCHours(0,0,0,0);
    return date;
  }

  static daysOfYear(year: number) {    
    return this.isLeapYear(year) ? 366 : 365;
  }

  static isLeapYear(year: number) {
    return year % 400 === 0 || (year % 100 !== 0 && year % 4 === 0);
  }

}