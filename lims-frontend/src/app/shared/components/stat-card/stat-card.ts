import { Component, Input } from '@angular/core';
import { AppIcon } from '../app-icon/app-icon';

@Component({
  selector: 'app-stat-card',
  imports: [AppIcon],
  templateUrl: './stat-card.html',
  styleUrl: './stat-card.css'
})
export class StatCard {
  @Input() label = '';
  @Input() value: string | number | null = 0;
  @Input() icon = '📊';
  @Input() color = '#2563EB'; // Can pass tailwind hex if needed
  @Input() prefix = '';
}
