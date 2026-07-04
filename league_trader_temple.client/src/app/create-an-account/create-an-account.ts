import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../Injectable/auth-service';

@Component({
  selector: 'app-create-an-account',
  standalone: false,
  templateUrl: './create-an-account.html',
  styleUrl: './create-an-account.css',
})
export class CreateAnAccount {
  public errorMessage: string = '';
  public username: string = '';
  public password: string = '';
  public confirmPassword: string = '';
  constructor(private http: HttpClient, private router: Router, private authService: AuthService) { }

  public OnSubmit(): void {
    this.errorMessage = '';
    if (this.password != this.confirmPassword) {
      this.errorMessage = 'Password do not match.'
      return;
    }
    this.http.post<{
      success: boolean;
      user: {
        id: number;
        username: string;
      };
    }>("/Account/register", {
      username: this.username,
      password: this.password
    }).subscribe({
      next: (response) => {
        this.authService.setCurrentUser(response.user)
        this.router.navigate(['/']);
      },
      error: (err) => {
        alert(err.error.message);
      },
    })
  }
}
