import { Component, OnInit } from '@angular/core';
import { Order } from '../../../core/classes/order.class';
import { Helper } from '../../../core/classes/helper.class';
import { SalesService } from '../../../core/services/salesService.service';
import { LogService } from '../../../core/services/logService.service'
import { TimePeriod } from 'src/app/core/timePeriod.enum';
import { Observable, fromEvent, interval } from 'rxjs';
import { map, mergeMap, mergeAll, } from 'rxjs/operators';
//import { map } from 'highcharts';

@Component({
  selector: 'app-manage',
  templateUrl: './manage.component.html',
  styleUrls: ['./manage.component.scss'],
  providers: [
    SalesService,
    LogService
  ]
})
export class ManageComponent implements OnInit {
  orders: Array<Order> = [];
  newItemDateCreated: Date = new Date();
  newItemAmount: number = 0;
  editId: number = -1;

  filterTimePeriod: TimePeriod; 
  filterStartDate: Date;
  filterEndDate: Date;

  /// TODO: For test data only
  from: Date;
  to: Date;
  maxAmount: number = 0;
  maxQty: number = 0;
  progress: number = 0;  
  ///

  constructor(private salesService: SalesService, private logService: LogService) { 
    /// TODO: For test data only
    var tenYearsAgo = new Date();
    tenYearsAgo.setMonth(tenYearsAgo.getMonth() - 6);
    this.from = tenYearsAgo;
    this.to = new Date();
    ///
  }

  hightlight(hightlightElement, isHightlight) {
    hightlightElement.style.backgroundColor = (isHightlight ? 'lightgray' : '');
  }
  
  parseDateOrDefault(dateStr, defaultValue) {
    return Helper.parseDateOrDefault(dateStr, defaultValue);
  }

  setNewItemDateCreated(dateStr) {
    var res = Helper.parseDateOrDefault(dateStr, null);
    if(res) {
      return;
    }

    this.newItemDateCreated = res;
  }

  validateItem(newItemAmount: number, newItemDateCreated: Date) {
    if(newItemAmount == null || newItemAmount <=0 || !newItemDateCreated) {
      return false;
    }

    return true;
  }

  deleteItem(id: number) {
    if (id <= 0) {
      this.logService.warning(`deleteItem. incorrect parameters: ${id}`);
      return;
    }

    var self = this;
    this.salesService.deleteSalesOrder(id).subscribe({
      next(response) { 
        self.logService.info(`deleteItem. response: ${response}`);

        // TODO: Bad method. Full read from DB.
        self.refreshData();
      },
      error(err) { 
        self.logService.error(`deleteItem. error: ${err}`); 
      },
      complete() { 
        self.logService.info(`deleteItem. complete.`); 
      }
    });
  }

  updateItem(id: number, newItemAmount: number, newItemDateCreated: Date) {    
    var self = this;
    this.salesService.updateSalesOrder(new Order(id, +newItemAmount, newItemDateCreated)).subscribe({
      next(response) { 
        self.logService.info(`updateItem. response: ${response}`); 

        // TODO: Bad method. Full read from DB.
        self.refreshData();
      },
      error(err) { 
        self.logService.error(`updateItem. error: ${err}`); 
      },
      complete() { 
        self.logService.info(`updateItem. complete.`); 
      }
    });
  }

  addItem(itemAmount: number, dateCreated: Date) {
    if(itemAmount == null || !dateCreated) {
      this.logService.warning(`addItem. incorrect parameters: ${itemAmount}, ${dateCreated}`);
      return;
    }

    var self = this;
    this.salesService.addSalesOrder(new Order(0, +itemAmount, dateCreated)).subscribe({
      next(response) { 
        self.logService.info(`addItem. response: ${response}`); 

        // TODO: Bad method. Full read from DB.
        self.refreshData();
      },
      error(err) { 
        self.logService.error(`addItem. error: ${err}`); 
      },
      complete() { 
        self.logService.info(`addItem. complete.`); 
      }
    });
  }

  orderItemTrackBy(index: number, item: Order) {
    return item.id;
  }

  inputFloatOnly(event): boolean {
    return Helper.checkInputDecimal(event)
  }

  /// TODO: For test data only
  inputIntOnly(event): boolean {
    return Helper.checkInputInt(event)
  }

  getProgress() {
    return `${this.progress}%`
  }

  generateTestItems(from: Date, to: Date, maxAmount: number, maxQty: number) {
    var self = this;

    var days = Math.round((to.valueOf() - from.valueOf())/(1000 * 3600 * 24));
    var curDateCreated = new Date(from);
    let obOrders: Observable<Order> = null; 
    for(var d = 0; d < days; d++) {
      let ordersCount = Math.floor(Math.random() * maxQty) + 1;
      for (var i=0; i< ordersCount; i++) { 
        let amt = Math.floor(Math.random() * maxAmount) + 1;
        let curDateCreated2 = new Date(curDateCreated);
        let orr = new Order(0, amt, curDateCreated2);
        if (obOrders == null) {
          obOrders = self.salesService.addSalesOrder(orr);
        } else {
          let ii = d;
          // TODO: no call stack error - no progressbar :( .
          //obOrders = obOrders.pipe(mergeMap(z=> self.salesService.addSalesOrder(orr), 1));
          
          // TODO: call stack error.
          obOrders = obOrders.pipe(mergeMap(z=> {
            self.progress = (ii/days) * 100;
            return self.salesService.addSalesOrder(orr);
          }, 1));         
        }
      }
      curDateCreated.setDate(curDateCreated.getDate() + 1);
    }

    obOrders.subscribe({
      next(response) { 
        self.logService.info(`generateTestItems. response: ${response}`);         
      },
      error(err) { 
        self.logService.error(`generateTestItems. error: ${err}`); 
      },
      complete() { 
        self.logService.info(`generateTestItems. complete.`); 
        self.progress = 100;
        self.refreshData();
      }
    });
  }

  validateGenerate(from: Date, to: Date, maxAmount: number, maxQty: number): Boolean {
    if (!from || !to || maxAmount === null || maxAmount <= 0 || maxQty === null || maxQty <= 0) {
      return false;
    }

    return true;
  }
  ///

  isEdit(id: number) : boolean {
    return this.editId === id;
  }

  cancelEdit() {
    this.editId = -1;
  }

  initRefreshData(timePeriod: TimePeriod, startDate: Date, endDate: Date, onlyGrouping: Boolean) {
    this.filterTimePeriod = timePeriod;
    this.filterStartDate = startDate;
    this.filterEndDate = endDate;

    this.refreshData();
  }

  refreshData() {
    if (this.filterTimePeriod == null) {
      this.logService.warning(`refreshData. incorrect parameters: ${this.filterTimePeriod}, ${this.filterStartDate}, ${this.filterEndDate}`);
      return;
    }

    var self = this;
    this.salesService.getSalesOrders(this.filterStartDate, this.filterEndDate).subscribe({
      next(response) {         
        self.logService.info(`refreshData. response: ${response}`); 
        self.orders = response;
      },
      error(err) { 
        self.logService.error(`refreshData. error: ${err}`); 
        self.orders = [];
      },
      complete() { 
        self.logService.info(`refreshData. complete.`); 
      }
    });
  }

  ngOnInit(): void {    
  }

}
