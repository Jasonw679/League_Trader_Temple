import { Component} from '@angular/core';
import { Router } from '@angular/router';

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
  setId: string;
  collectorNumber: number;
  rarity: string;
  faction: string;
  type: string;
  orientation: string;
  stats: RiftboundCardStats;
  image: string;
  imageThumb: RiftboundCardImageThumb;
  imageBlurDataUrl: string;
  isBanned: boolean;
}

export interface RiftboundCardStats {
  energy?: number | null;
  might?: number | null;
  power?: number | null;
}

export interface RiftboundCardImageThumb {
  small: string;
  medium: string;
  large: string;
}
@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})
export class App{
  constructor(private router: Router) {}

  loadCards(search = ''): void {
    this.router.navigate(['search'], { queryParams: { search } });
  }
}
