import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { TimePeriod } from '../../../core/timePeriod.enum';
import { Helper } from '../../../core/classes/helper.class'
import { LogService } from '../../../core/services/logService.service'

@Component({
  selector: 'app-filter',
  templateUrl: './filter.component.html',
  styleUrls: ['./filter.component.scss'],
  providers:[LogService]
})
export class FilterComponent implements OnInit {
  @Input() showOnlyDateFilter: Boolean = false;
  @Input() isZoomed: Boolean = false;

  @Output() updateData = new EventEmitter<[TimePeriod, Date, Date, Boolean]>();
  @Output() resetZoom = new EventEmitter();  

  startDate: Date;
  endDate: Date;
  timePeriods: Array<TimePeriod> = [];
  /* TODO: Does not work
   (()=> {
    let retValue: Array<TimePeriod> = [];
    for(let val in TimePeriod) {      
      if(typeof TimePeriod[val] === 'number') {
        console.log(val, TimePeriod[val], typeof TimePeriod[val]);
        retValue.push(TimePeriod[TimePeriod[val]]);
      }
      return retValue;
  }})();*/
  timePeriod: string = TimePeriod[TimePeriod.Quarter];

  constructor(private logService: LogService) {
    var nYearsAgo = new Date();
    nYearsAgo.setFullYear(nYearsAgo.getFullYear() - 1);
    this.startDate = nYearsAgo;
    this.endDate = new Date();
    for (let val in TimePeriod) {
      if (typeof TimePeriod[val] === 'number') {
        this.timePeriods.push(TimePeriod[TimePeriod[val]]);
      }
    }
  }

  timePeriodOnChange(val, mode) {
    this.logService.info(`timePeriodOnChange. ${val}, ${mode}`);
    let shouldRefreshData = false;
    if (mode == 0) {
      let newVal = Helper.parseDateOrDefault(val, this.startDate);
      if (newVal != null && newVal != this.startDate) {
        this.startDate = newVal;
        shouldRefreshData = true;
      }
    } else if (mode == 1) {
      let newVal = Helper.parseDateOrDefault(val, this.endDate);
      if (newVal != null && newVal != this.startDate) {        
        this.endDate = newVal; 
        shouldRefreshData = true;
      }
    }

    if (shouldRefreshData) {
      this.refreshData(false);
    }
  }

  refreshData(onlyGrouping) {
    this.logService.info(`refreshData. ${onlyGrouping}, ${this.updateData}`);
    if (this.updateData == null) {
      return;
    }
    
    let timePeriod = Helper.parseTimePeriodByName(this.timePeriod);
    // rule: [from, to)
    var ed = new Date(this.endDate);
    ed.setDate(ed.getDate() + 1);
    ed = Helper.clearUtcTimeOfDate(ed);
    this.updateData.emit([timePeriod, Helper.clearUtcTimeOfDate(new Date(this.startDate)), ed, onlyGrouping]);
  }  

  reset() {
    if (this.resetZoom == null) {
      return;
    }

    this.resetZoom.emit();
  }

  ngOnInit(): void {
    this.refreshData(false);
  }
}
