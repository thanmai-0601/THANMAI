import { Component, ElementRef, ViewChild, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api';
import { AppIcon } from '../app-icon/app-icon';
import { AuthService } from '../../../core/services/auth';

interface Message {
  text: string;
  isBot: boolean;
  timestamp: Date;
}

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule, AppIcon],
  templateUrl: './chatbot.html',
  styleUrl: './chatbot.css',
  encapsulation: ViewEncapsulation.None
})
export class Chatbot {
  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;
  
  isOpen = false;
  message = '';
  loading = false;
  messages: Message[] = [
    { text: 'Hello! I am your NexaLife AI Assistant. How can I help you today?', isBot: true, timestamp: new Date() }
  ];

  constructor(private api: ApiService, private authService: AuthService) {}

  toggleChat() {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      setTimeout(() => this.scrollToBottom(), 100);
    }
  }

  onKeydown(event: KeyboardEvent) {
    if (event.key === 'Enter') {
      event.preventDefault(); // Strongly prevent default behavior
      this.sendMessage();
    }
  }

  sendMessage() {
    if (!this.message.trim() || this.loading) return;

    const userMsg = this.message;
    this.messages.push({ text: userMsg, isBot: false, timestamp: new Date() });
    this.message = '';
    this.loading = true;
    this.scrollToBottom();

    this.api.post<any>('chat', { message: userMsg }).subscribe({
      next: (res) => {
        this.messages.push({ text: res.response, isBot: true, timestamp: new Date() });
        this.loading = false;
        this.scrollToBottom();
      },
      error: (err) => {
        this.messages.push({ text: 'Sorry, I encountered an error. Please try again.', isBot: true, timestamp: new Date() });
        this.loading = false;
        this.scrollToBottom();
      }
    });
  }

  scrollToBottom(): void {
    try {
      this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight;
    } catch(err) { }
  }
}
