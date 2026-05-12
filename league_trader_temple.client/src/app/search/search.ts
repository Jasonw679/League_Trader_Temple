import { Component, signal, OnInit, NgModule } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { RiftboundCardPage, RiftboundCard } from '../app';

@Component({
  selector: 'app-search',
  standalone: false,
  templateUrl: './search.html',
  styleUrl: './search.css',
})
export class Search implements OnInit {
  public readonly cards = signal<RiftboundCard[]>([]);
  public readonly isLoadingCards = signal(false);
  public readonly cardLoadError = signal('');
  constructor(private http: HttpClient, private route: ActivatedRoute) { }

  public ngOnInit(): void {
    const search = this.route.snapshot.queryParamMap.get('search') ?? '';
    this.loadCards(search);
  }

  public loadCards(search = ''): void {
    const params: Record<string, string | number> = {
      sort: 'collector_number'
    };

    if (search) {
      params['search'] = search;
    }

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
