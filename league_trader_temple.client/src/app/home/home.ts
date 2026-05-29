import { Component, signal, NgModule } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RiftboundCard, RiftboundCardPage } from '../app';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  public readonly cards = signal<RiftboundCard[]>([]);
  public readonly cardLoadError = signal('');
  public readonly isLoadingCards = signal(false)
  constructor(private http: HttpClient) { }

  public ngOnInit(): void {
    this.loadCards();
  }

  public loadCards(): void {
    const params: Record<string, string | number> = {
      size: 10,
      sort: 'visits',
      dir: -1
    };

    this.isLoadingCards.set(true);
    this.cardLoadError.set('');

    this.http.get<RiftboundCardPage>('/riftboundcards', { params }).subscribe({
      next: (page) => {
        this.cards.set(page.items);
        this.isLoadingCards.set(false);
      },
      error: () => {
        this.cards.set([]);
        this.cardLoadError.set('Unable to load Riftbound cards right now.');
        this.isLoadingCards.set(false);
      }
    });
  }
}
