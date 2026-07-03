import { Component, signal, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../auth-service';

@Component({
  selector: 'app-header',
  standalone: false,
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  constructor(public auth: AuthService) { }
  public readonly searchTerm = signal('');
  @Output() search = new EventEmitter<string>();
  public onSearchInput(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
  }

  public searchCards(): void {
    this.search.emit(this.searchTerm());
  }
}
