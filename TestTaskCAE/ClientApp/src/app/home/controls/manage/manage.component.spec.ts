import { async, ComponentFixture, TestBed, getTestBed, inject } from '@angular/core/testing';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';

import { SalesService } from '../../../core/services/salesService.service';
import { LogService } from '../../../core/services/logService.service';
import { ManageComponent } from './manage.component';
import { FilterComponent } from '../filter/filter.component';
import { TimePeriod } from 'src/app/core/timePeriod.enum';

describe('ManageComponent', () => {
  let component: ManageComponent;
  let fixture: ComponentFixture<ManageComponent>;

  let salesService: SalesService;
  let logService: LogService;
  
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ 
        FilterComponent,
        ManageComponent 
      ],
      providers: [
        SalesService,
        LogService
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
    fixture = TestBed.createComponent(ManageComponent);
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
 
  it('parameters should be empty', () => {
    expect(component.orders).toEqual([]);
    expect(component.newItemDateCreated).not.toBeNull();
    expect(component.newItemAmount).toBe(0);
    expect(component.editId).toBe(-1);
    expect(component.filterTimePeriod).toBe(TimePeriod.Quarter);
    expect(component.filterStartDate).not.toBeNull();
    expect(component.filterEndDate).not.toBeNull();
    expect(component.from).not.toBeNull();
    expect(component.to).not.toBeNull();
    expect(component.maxAmount).toBe(0);
    expect(component.maxQty).toBe(0);
    expect(component.progress).toBe(0);
  });

  it('check validate item function - valid data', () => {
    var result = component.validateItem(1, new Date());
    expect(result).toBeTrue();
  });

  it('check validate item function - invalid data', () => {
    var result = component.validateItem(null, new Date());
    expect(result).toBeFalse();
  });

  it(`add item - correct data`,
    async(inject([HttpClient, HttpTestingController], (http: HttpClient, backend: HttpTestingController) => {
        component.addItem(25, new Date());

        backend.expectOne({
          url: '/api/sales',
          method: 'POST'
        });
      })
    )
  );

  it(`add item - bad data`,
    async(inject([HttpClient, HttpTestingController], (http: HttpClient, backend: HttpTestingController) => {
        component.addItem(null, null);

        backend.expectNone({
          url: '/api/sales',
          method: 'POST'
        });
      })
    )
  );

  it(`delete item`,
    async(inject([HttpClient, HttpTestingController], (http: HttpClient, backend: HttpTestingController) => {
        component.addItem(null, null);

        backend.expectNone({
          url: '/api/sales/{id}',
          method: 'DELETE'
        });
      })
    )
  );

  it(`update item`,
    async(inject([HttpClient, HttpTestingController], (http: HttpClient, backend: HttpTestingController) => {
        component.updateItem(1, 23, new Date());

        backend.expectNone({
          url: '/api/sales/',
          method: 'PUT'
        });
      })
    )
  );

  it(`generate test data`,
    async(inject([HttpClient, HttpTestingController], (http: HttpClient, backend: HttpTestingController) => {
        var dateTo = new Date();
        var dateFrom = new Date(dateTo);
        dateFrom.setDate(dateFrom.getDate() -1);
        component.generateTestItems(dateFrom, dateTo, 1200, 1);

        backend.expectOne({
          url: '/api/sales',
          method: 'POST'
        });
      })
    )
  );

  //generateTestItems(from: Date, to: Date, maxAmount: number, maxQty: number)

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
