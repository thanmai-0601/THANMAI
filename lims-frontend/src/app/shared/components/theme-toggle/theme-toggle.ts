import { Component } from '@angular/core';
import { ThemeService } from '../../../core/services/theme';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-theme-toggle',
  imports: [AsyncPipe],
  templateUrl: './theme-toggle.html',
  styleUrl: './theme-toggle.css'
})
export class ThemeToggle {
  constructor(public themeService: ThemeService) { }
}
