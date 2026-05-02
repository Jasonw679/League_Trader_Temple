import { HttpClient } from '@angular/common/http';
import { Component, signal, OnInit } from '@angular/core';

export interface RiftboundCardPage {
  items: RiftboundCard[];
  total: number;
  page: number;
  size: number;
  pages: number;
}

export interface RiftboundCard {
  id: string;
  name: string;
  riftboundId: string;
  publicCode: string;
  collectorNumber: number;
  classification: RiftboundCardClassification;
  attributes: RiftboundCardAttributes;
  set: RiftboundCardSet;
  media: RiftboundCardMedia;
}

export interface RiftboundCardAttributes {
  energy?: number | null;
  might?: number | null;
  power?: number | null;
}

export interface RiftboundCardClassification {
  type: string;
  supertype?: string | null;
  rarity: string;
  domain: string[];
}

export interface RiftboundCardSet {
  id?: string;
  setId?: string;
  label: string;
}

export interface RiftboundCardMedia {
  imageUrl: string;
  artist: string;
  accessibilityText: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})
export class App implements OnInit {
  public readonly cards = signal<RiftboundCard[]>([]);
  public readonly cardLoadError = signal('');
  public readonly isLoadingCards = signal(false);
  public readonly totalCards = signal(0);

  constructor(private http: HttpClient) {}

  protected readonly title = signal('league_trader_temple.client');

  public ngOnInit(): void {
    this.loadCards();
  }

  public loadCards(search = ''): void {
    const trimmedSearch = search.trim();
    const params: Record<string, string | number> = {
      size: 24,
      sort: 'collector_number',
      setId: 'ogn'
    };

    if (trimmedSearch) {
      params['search'] = trimmedSearch;
    }

    this.isLoadingCards.set(true);
    this.cardLoadError.set('');

    this.http.get<RiftboundCardPage>('/riftboundcards', { params }).subscribe({
      next: (page) => {
        this.cards.set(page.items);
        this.totalCards.set(page.total);
        this.isLoadingCards.set(false);
      },
      error: () => {
        this.cards.set([]);
        this.totalCards.set(0);
        this.cardLoadError.set('Unable to load Riftbound cards right now.');
        this.isLoadingCards.set(false);
      }
    });
  }
}
