import { async, ComponentFixture, TestBed, getTestBed, inject } from '@angular/core/testing';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';

import { SalesService } from '../../../core/services/salesService.service';
import { LogService } from '../../../core/services/logService.service';
import { ReportComponent } from './report.component';
import { FilterComponent } from '../filter/filter.component';

describe('ReportComponent', () => {
  let component: ReportComponent;
  let fixture: ComponentFixture<ReportComponent>;

  let salesService: SalesService;
  let logService: LogService;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ 
        FilterComponent,
        ReportComponent 
      ],
      providers: [
        LogService,
        SalesService
      ],
      imports: [
        FormsModule,
        HttpClientModule,
        HttpClientTestingModule
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should use SalesService', () => {
    salesService = TestBed.inject(SalesService);
    expect(salesService).not.toBeNull();
  });

  it('should use LogService', () => {
    logService = TestBed.inject(LogService);
    expect(logService).not.toBeNull();
  });  

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('parameters should be empty', () => {
    expect(component.orders).toBe(undefined);
    expect(component.groupedOrders).toBe(undefined);
    expect(component.chartOptions).toBe(undefined);
    expect(component.isZoomed).toBe(false);
    expect(component.filterTimePeriod).toBe(0);
    expect(component.filterStartDate).not.toBe(undefined);
    expect(component.filterEndDate).not.toBe(undefined);
  });
});
