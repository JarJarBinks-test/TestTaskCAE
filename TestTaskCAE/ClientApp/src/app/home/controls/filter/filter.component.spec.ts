import { async, ComponentFixture, TestBed, getTestBed, inject } from '@angular/core/testing';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';

import { LogService } from '../../../core/services/logService.service';
import { FilterComponent } from './filter.component';
import { TimePeriod } from 'src/app/core/timePeriod.enum';

describe('FilterComponent', () => {
  let component: FilterComponent;
  let fixture: ComponentFixture<FilterComponent>;

  let logService: LogService;

  
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ 
        FilterComponent 
      ],
      providers: [
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

  it('should use LogService', () => {
    logService = TestBed.inject(LogService);
    expect(logService).not.toBeNull();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('parameters should be empty', () => {
    expect(component.showOnlyDateFilter).toBeFalse();
    expect(component.isZoomed).toBeFalse();
    expect(component.updateData).not.toBeNull();
    expect(component.resetZoom).not.toBeNull();

    expect(component.startDate).not.toBeNull();
    expect(component.endDate).not.toBeNull();
    expect(component.timePeriods.length).toBe(4);
    expect(component.timePeriod).toBe(TimePeriod[TimePeriod.Quarter]);
  });
});
