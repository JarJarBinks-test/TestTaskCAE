import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpEvent, HttpInterceptor, HttpHandler, HttpRequest } from '@angular/common/http'
import { LogService } from './logService.service'
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { Order } from '../classes/order.class';

@Injectable({
  providedIn: 'root'
})
export class SalesService {
  private serviceUri: string = "/api/sales";

  constructor(private httpClient: HttpClient, private logService: LogService) {

  }

  private getRequestOption(requestType: Number) {
    // TODO: For future use. Auth module for example.
  }

  getSalesOrders(startDate, endDate): Observable<Array<Order>> {
    var startDateParam = startDate != null ? startDate.toISOString() : '';
    var endDateParam = endDate != null ? endDate.toISOString() : '';
    return this.httpClient.get<Array<Order>>(this.serviceUri + `/${startDateParam}/${endDateParam}`).pipe(
      catchError(this.handleError)
    );
  }

  addSalesOrder(o: Order): Observable<Order> {
    return this.httpClient.post<Order>(this.serviceUri, o)
    .pipe(
      catchError(this.handleError)
    );
  }

  updateSalesOrder(o: Order) {
    return this.httpClient.put<Order>(this.serviceUri, o)
    .pipe(
      catchError(this.handleError)
    );
  }

  deleteSalesOrder(id: Number) {
    return this.httpClient.delete(this.serviceUri + `/${id}`)
    .pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse) {
    this.logService.error(error.error instanceof ErrorEvent ?
        `An error occurred: ${error.error.message}` : 
        `Backend returned code ${error.status}, body was: ${error.error}`);
        
    return throwError('Something bad happened; please try again later.');
  }

}