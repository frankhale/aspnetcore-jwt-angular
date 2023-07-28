import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable, catchError, switchMap, throwError } from 'rxjs';
import { AuthService, TokenResponse } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class TokenInterceptor implements HttpInterceptor {

  constructor(private authService: AuthService) {
  }

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    console.log("REQUEST URL", request.url);

    if (request.url.endsWith('token') ||
      request.url.endsWith('refresh') ||
      request.url.endsWith('/')) {
      return next.handle(request);
    }

    if (!this.isTokenExpired(this.authService.token)) {
      console.log('Setting authorization header for request!');
      request = request.clone(
        {
          setHeaders: {
            Authorization: `Bearer ${this.authService.token}`
          }
        });
      return next.handle(request);
    } else {
      console.log("REFRESHING TOKEN!");
      return this.authService.refresh().pipe(
        switchMap((newToken: TokenResponse) => {
          console.log("NEW TOKEN", newToken);
          // update the request with the new token
          request = request.clone(
            {
              setHeaders: {
                Authorization: `Bearer ${newToken.accessToken}`
              }
            });

          return next.handle(request);
        }),
        catchError(error => {
          console.error(error);
          return throwError(() => { error });
        })
      );
    }
  }


  isTokenExpired(token: string | null): boolean {
    if (!token) {
      return false;
    }

    const expiry = (JSON.parse(atob(token.split('.')[1]))).exp;
    const isExpired = (Math.floor((new Date).getTime() / 1000)) >= expiry;
    //console.log("TOKEN EXPIRY", expiry);
    console.log("TOKEN EXPIRED", isExpired);

    return isExpired;
  }
}
