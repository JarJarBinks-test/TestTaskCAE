import { Component, OnInit } from '@angular/core';
import { Order } from '../../../core/classes/order.class';
import { SalesService } from '../../../core/services/salesService.service';
import { LogService } from '../../../core/services/logService.service'
import { TimePeriod } from '../../../core/timePeriod.enum';
import * as Highcharts from 'highcharts';

@Component({
  selector: 'app-manage',
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.scss'],
  providers: [
    SalesService,
    LogService
  ]
})
export class ReportComponent implements OnInit {
  Highcharts: typeof Highcharts = Highcharts;
  
  orders: Array<Order>;
  groupedOrders: [Array<number>, Array<number>];

  chartOptions: Highcharts.Options;
  isZoomed: Boolean = false;

  filterTimePeriod: TimePeriod; 
  filterStartDate: Date;
  filterEndDate: Date;

  constructor(private salesService: SalesService, private logService: LogService) { 
  }

  groupData(orders: Array<Order>): [Array<number>, Array<number>] { //[Array<[number, number]>, Array<[number, number]>] {
    if(orders.length == 0) {
      return;
    }

    let simpleOrders =  orders.map(function (obj) { 
      let dt = new Date(obj.dateCreated);
      //let utcDate = Date.UTC(dt.getFullYear(), dt.getMonth(), dt.getDate());
      return {
        amount: obj.amount,
        dateCreated: dt//utcDate
      };
    });

    let simpleOrdersVOf =  orders.map(function (obj) { 
      let dt = new Date(obj.dateCreated);
      return {
        amount: obj.amount,
        dateCreated: dt.valueOf()
      };
    });

    let minDate = this.filterStartDate; //keys[0];
    let maxDate = this.filterEndDate;//keys[keys.length -1];
    

    let resultAmount = new Array<number>();
    let resultQty = new Array<number>();
    let currMinDate = new Date(minDate);  
    
    let groupByDays = (dayCount: number) => {
      let ticksInDay = 1000 * 3600 * 24;
      let ticksInPeriod = ticksInDay * dayCount;

      let periods = (maxDate.valueOf() - minDate.valueOf()) / ticksInPeriod;
      for (let per = 0; per <= periods; per++) {
        let currVof = currMinDate.valueOf();
        var dataInPeriod = simpleOrdersVOf.filter(function(value, index, arr) { 
          var reslt = (value.dateCreated - currVof);
          return reslt < ticksInPeriod && reslt >= 0;
        });
        var totalAmountInperiod = dataInPeriod.reduce((amt, obj) => amt + obj.amount, 0);
        var countInPeriod = dataInPeriod.length;

        resultAmount.push(totalAmountInperiod);
        resultQty.push(countInPeriod);
        currMinDate.setDate(currMinDate.getDate() + dayCount);
      }
    }

    let groupByMonths = (monthCount: number) => {
      let periods = (maxDate.getFullYear() * 12 + maxDate.getMonth() - (minDate.getFullYear() * 12 + minDate.getMonth())) / monthCount;
      for (let per = 0; per <= periods; per++) {
        let currVof = currMinDate.getFullYear() * 12 + currMinDate.getMonth();
        var dataInPeriod = simpleOrders.filter(function(value, index, arr) { 
          var reslt = (value.dateCreated.getFullYear() * 12 + value.dateCreated.getMonth() - currVof);
          return reslt < monthCount && reslt >= 0;
        });
        var totalAmountInperiod = dataInPeriod.reduce((amt, obj) => amt + obj.amount, 0);
        var countInPeriod = dataInPeriod.length;

        resultAmount.push(totalAmountInperiod);
        resultQty.push(countInPeriod);
        currMinDate.setMonth(currMinDate.getMonth() + monthCount);
      }
    }

    if (this.filterTimePeriod == TimePeriod.Day) {
      groupByDays(1);
    } else if (this.filterTimePeriod == TimePeriod.Week) {
      groupByDays(7);
    } else if (this.filterTimePeriod == TimePeriod.Month) {
      groupByMonths(1);
    } else if (this.filterTimePeriod == TimePeriod.Quarter) {
      groupByMonths(3);
    }

    return [resultAmount, resultQty];
  }

  refreshChartOptions() {
    var chartOptions: Highcharts.Options;
    var startPoint = Date.UTC(this.filterStartDate.getFullYear(), this.filterStartDate.getMonth(), this.filterStartDate.getDate());
    var pointInterval = null;
    var pointIntervalUnit = null;
    if (this.filterTimePeriod == TimePeriod.Day) {
      pointInterval = 1;
      pointIntervalUnit = 'day';
    } else if (this.filterTimePeriod == TimePeriod.Week) {
      pointInterval = 7;
      pointIntervalUnit = 'day';
    } else if (this.filterTimePeriod == TimePeriod.Month) {
      pointInterval = 1;
      pointIntervalUnit = 'month';
    } else if (this.filterTimePeriod == TimePeriod.Quarter) {
      pointInterval = 3;
      pointIntervalUnit = 'month';
    }
    var self = this;
    var colorSum = "#EDC240";
    var colorSales = "#AFD8F8";
    var colorY0Sum = "#000000";
    var colorY1Sum = "#000000";
    chartOptions = {
      chart: {
        zoomType: 'xy',
        resetZoomButton: {
          theme: {
              display: 'none'
          }
        }
      },
      title: {
        text: null
      },
      tooltip: {
        formatter: function(f) {
          var date = new Date(this.x);
          return `<span><b>${this.point.series.name.split(' ')[0]} on ${date.getFullYear()}/${date.getMonth()}/${date.getDate()}: ${this.point.y}</b></span>`;
        }
      },      
      xAxis: [{  
        title: {
          text: "Time"
        },    
        type: 'datetime',
        labels: {
          formatter: function() {
            let dt = new Date(this.value);
            
            return `${dt.getFullYear()}/${dt.getMonth()}/${dt.getDate()}`;
          }
          //format: '{value: %Y/%b/%e}'
        },
        crosshair: true,
        events: {
          afterSetExtremes (e) {
            if (e.trigger === "zoom") {
              self.isZoomed = (e.userMax !== undefined ||  e.userMin !== undefined);
            }
          }
        }
      }],
      yAxis: [{
        title: {
          text: "Sum (In Thousands)",
          style: {
            color: colorY0Sum
          }
        },
        labels: {
          format: '{value}',
          style: {
            color: colorY0Sum
          }
        }
      }, {
        title: {
          text: "Count",
          style: {
            color: colorY1Sum
          }
        },
        labels: {
          format: '{value}',
          style: {
            color: colorY1Sum
          }
        },
        opposite: true
      }],
      legend: {
        layout: 'vertical',
        align: 'right',
        x: -20,
        y: 0,
        verticalAlign: 'top',        
        floating: true,
        //backgroundColor: Highcharts.defaultOptions.legend.backgroundColor || 'gray'
      },

      series: [{
          name: 'Sum $/K',
          data: this.groupedOrders[0],
          type: 'line',
          yAxis: 0,
          pointInterval: pointInterval,
          pointIntervalUnit: pointIntervalUnit,
          pointStart: startPoint,
          color: colorSum,
          zIndex: 2
        }, {
          name: 'Sales',
          data: this.groupedOrders[1],
          type: 'column',
          yAxis: 1,
          pointInterval: pointInterval,
          pointIntervalUnit: pointIntervalUnit,
          pointStart: startPoint,
          color: colorSales,
          zIndex: 1
        }
      ]
    };

    this.chartOptions = chartOptions;
  }

  initRefreshData(timePeriod: TimePeriod, startDate: Date, endDate: Date, onlyGrouping: Boolean) {
    this.filterTimePeriod = timePeriod;
    this.filterStartDate = startDate;
    this.filterEndDate = endDate;

    this.refreshData(onlyGrouping);
  }

  groupAndRefreshData() {
    this.groupedOrders = this.groupData(this.orders);
    this.refreshChartOptions();
  }

  refreshData(onlyGrouping: Boolean) {
    if (this.filterTimePeriod == null) {
      this.logService.warning(`refreshData. incorrect parameters: ${this.filterTimePeriod}, ${this.filterStartDate}, ${this.filterEndDate}`);
      return;
    }

    if (onlyGrouping) {
      this.groupAndRefreshData();
      return;
    }

    var self = this;
    this.salesService.getSalesOrders(this.filterStartDate, this.filterEndDate).subscribe({
      next(response) {         
        self.logService.info(`refreshData. response: ${response}`); 
        self.orders = response;
        self.groupAndRefreshData();        
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

  resetZoom() {
    if (Highcharts.charts.length == 0) {
      return;
    }

    Highcharts.charts[0].zoomOut();
  }

  ngOnInit(): void {
    if (Highcharts.charts.length == 0) {
      return;
    }

    //Highcharts.charts[0].resetZoomButton.hide()
  }
}
