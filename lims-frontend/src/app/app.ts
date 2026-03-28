import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Toast } from './shared/components/toast/toast';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Toast],
  template: `
    <router-outlet></router-outlet>
    <app-toast />
  `
})
export class AppComponent {
  constructor() {
    // Purge legacy local storage keys
    localStorage.removeItem('lims_user');
    localStorage.removeItem('lims_token');
    localStorage.removeItem('lims_theme');
    localStorage.removeItem('user');
  }
}
