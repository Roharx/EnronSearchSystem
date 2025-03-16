import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, IonicModule, HttpClientModule],
  templateUrl: './home.page.html',
  styleUrls: ['./home.page.scss'],
})
export class HomePage {
  searchQuery = '';
  searchResults: any[] = [];

  constructor(private http: HttpClient) {}

  searchFiles() {
    if (!this.searchQuery.trim()) return;

    this.http.get<any[]>(`http://localhost:8080/search?q=${this.searchQuery}`).subscribe((data) => {
      this.searchResults = data;
    });
  }

  downloadFile(fileId: number) {
    window.open(`http://localhost:8080/file/${fileId}`, '_blank');
  }
}
