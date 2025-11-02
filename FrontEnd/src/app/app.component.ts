import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'book-manager-frontend';
  navCollapsed = true;

  toggleNav(): void {
    this.navCollapsed = !this.navCollapsed;
  }

  closeNav(): void {
    this.navCollapsed = true;
  }
}
