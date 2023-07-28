import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
}

@Injectable()
export class AuthService {
  private jwtTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);
  private refreshToken: string = "";

  constructor(private http: HttpClient) { }

  get token(): string | null {
    //console.log("TOKEN: ", this.jwtTokenSubject.value);
    return this.jwtTokenSubject.value;
  }

  login(): Observable<any> {
    return this.http.post<TokenResponse>('https://localhost:7001/token', {}).pipe(
      tap(response => {
        this.jwtTokenSubject.next(response.accessToken);
        this.refreshToken = response.refreshToken;
      })
    );
  }

  refresh(): Observable<any> {
    const params = new HttpParams()
      .set('refreshToken', this.refreshToken);


    return this.http.post<TokenResponse>('https://localhost:7001/refresh', null, {
      params
    }).pipe(
      tap(response => {
        this.jwtTokenSubject.next(response.accessToken);
        this.refreshToken = response.refreshToken;
      })
    );
  }

  logout(): void {
    this.jwtTokenSubject.next(null);
    this.refreshToken = "";
    // Also consider making a request to the server to invalidate the refresh token
  }

  weatherforecast(): Observable<any> {
    return this.http.get('https://localhost:7001/weatherforecast', {});
  }
}
