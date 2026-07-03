import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface LoggedInUser {
  id: number;
  username: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<LoggedInUser | null>(null);

  currentUser$ = this.currentUserSubject.asObservable();

  isLoggedIn() {
    return this.currentUserSubject.value != null
  }

  setCurrentUser(user: LoggedInUser) {
    this.currentUserSubject.next(user);
  }

  getCurrentUser(): LoggedInUser | null {
    return this.currentUserSubject.value;
  }

  logout() {
    this.currentUserSubject.next(null);
  }
}
