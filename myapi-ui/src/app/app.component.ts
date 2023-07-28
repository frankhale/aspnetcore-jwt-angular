import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet
  ],
  providers: [],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'myapi-ui';

  constructor(private authService: AuthService) { }

  onLoginClick(): void {
    this.authService.login().subscribe(
      response => {
        //console.log(response);
      });
  }

  onWeatherForecastClick(): void {
    this.authService.weatherforecast().subscribe(
      response => {
        console.log(response);
      });
  }
}
