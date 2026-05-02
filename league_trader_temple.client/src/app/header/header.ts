import { Component, signal } from '@angular/core';
import { App } from '../app';

@Component({
  selector: 'app-header',
  standalone: false,
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  public readonly searchTerm = signal('');
  public onSearchInput(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
  }

  public searchCards(): void {
    (App as any).loadCards(this.searchTerm());
  }
}
