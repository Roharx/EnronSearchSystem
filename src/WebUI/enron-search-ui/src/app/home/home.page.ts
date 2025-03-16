import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule, AlertController } from '@ionic/angular';
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

  constructor(private http: HttpClient, private alertCtrl: AlertController) {}

  async searchFiles() {
    if (!this.searchQuery.trim()) return;

    this.searchResults = [];

    this.http.get<any[]>(`http://localhost:8080/search?q=${this.searchQuery}`).subscribe(async (data) => {
      if (data.length === 0) {
        this.searchResults = [];

        // Show Ionic alert when no results are found
        const alert = await this.alertCtrl.create({
          header: 'No Results',
          message: `The database does not have the word: ${this.searchQuery}`,
          buttons: ['OK'],
        });

        await alert.present();
      } else {
        this.searchResults = data;
      }
    });
  }

  downloadFile(fileId: number) {
    window.open(`http://localhost:8080/file/${fileId}`, '_blank');
  }
}
