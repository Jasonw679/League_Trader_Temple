import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth-service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  public username: string = '';
  public password: string = '';
  public errorMessage: string = '';
  constructor(private http: HttpClient, private router: Router, private authService: AuthService) { }

  public onLoginSubmit(event: Event): void {

    this.errorMessage = '';
    this.http.post<{
      exists: boolean;
      user: {
        id: number;
        username: string;
      };
}>(
        `/account`,
        {
          username: this.username,
          password: this.password
        }
      )
      .subscribe({
        next: (response) => {
          if (response.exists) {
            console.log('Login successful');
            this.authService.setCurrentUser(response.user);
            this.router.navigate(['/']);
          } else {
            alert('Invalid username or password.');
            this.router.navigate(['/login']);
          }
        },
        error: () => {
          alert(this.errorMessage = 'Unable to connect to the server.');
        },
      });
  }
}
