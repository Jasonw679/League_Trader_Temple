import { Component, Input} from '@angular/core';
import { RiftboundCard } from '../app'

@Component({
  selector: 'app-card',
  standalone: false,
  templateUrl: './card.html',
  styleUrl: './card.css',
})

export class Card {
  @Input() card!: RiftboundCard;
}
