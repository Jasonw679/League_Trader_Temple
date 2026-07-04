import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../Injectable/auth-service';

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

  ngOnInit() {
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/']);
    }
  }

  public onLoginSubmit(): void {

    this.errorMessage = '';
    this.http.post<{
      exists: boolean;
      user: {
        id: number;
        username: string;
      };
    }>(`/Account/login`,
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
            this.router.navigate(['login']);
          }
        },
        error: (err) => {
          alert(err.error);
        },
      });
  }
}
