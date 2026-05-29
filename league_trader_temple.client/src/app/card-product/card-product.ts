import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subscription } from 'rxjs';
import { RiftboundCard, RiftboundCardPage } from '../app';

@Component({
  selector: 'app-card-product',
  standalone: false,
  templateUrl: './card-product.html',
  styleUrls: ['./card-product.css'],
})
export class CardProduct implements OnInit, OnDestroy {
  private static readonly visitorStorageKey = 'league-trader-temple-user-id';

  productId: string | null = null;
  public readonly product = signal<RiftboundCard | null>(null);
  public readonly loading = signal<boolean>(false);
  error: string | null = null;

  private routeSub?: Subscription;
  private fetchSub?: Subscription;
  private visitSub?: Subscription;

  constructor(private route: ActivatedRoute, private http: HttpClient) {}

  ngOnInit(): void {
    this.routeSub = this.route.paramMap.subscribe((params) => {
      this.productId = params.get('id');
      this.error = null;

      if (this.productId) {
        this.loadProductById(this.productId);
      }
    });
  }

  private loadProductById(id: string): void {
    this.loading.set(true);
    this.fetchSub?.unsubscribe();
    const params = { id };
    this.fetchSub = this.http.get<RiftboundCardPage>('/riftboundcards', { params })
      .subscribe({
        next: (page) => {
          const product = page.items.length > 0 ? page.items[0] : null;
          this.product.set(product);
          this.error = product ? null : 'Card not found';
          if (product) {
            this.recordVisit(product.id);
          }
          this.loading.set(false);
        },
        error: (err) => {
          this.error = err?.message || 'Failed to load product';
          this.loading.set(false);
        }
      });
  }

  private recordVisit(cardId: string): void {
    const userId = this.getOrCreateVisitorId();

    this.visitSub?.unsubscribe();
    this.visitSub = this.http.post<void>(
      `/riftboundcards/${encodeURIComponent(cardId)}/visits`,
      { userId }
    ).subscribe({
      error: () => {
        // Visit tracking should not block card browsing.
      }
    });
  }

  private getOrCreateVisitorId(): string {
    const existingUserId = localStorage.getItem(CardProduct.visitorStorageKey);

    if (existingUserId) {
      return existingUserId;
    }

    const userId = crypto.randomUUID();
    localStorage.setItem(CardProduct.visitorStorageKey, userId);
    return userId;
  }

  ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
    this.fetchSub?.unsubscribe();
    this.visitSub?.unsubscribe();
  }
}
