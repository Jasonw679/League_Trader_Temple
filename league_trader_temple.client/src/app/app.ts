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
  riftboundId: string;
  publicCode: string;
  collectorNumber: number;
  classification: RiftboundCardClassification;
  attributes: RiftboundCardAttributes;
  text?: RiftboundCardText;
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

export interface RiftboundCardText {
  rich: string;
  plain: string;
  flavour?: string | null;
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
