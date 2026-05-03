import customtkinter as ctk
import psutil
import threading
import time
from datetime import datetime

# إعدادات المظهر
ctk.set_appearance_mode("dark")
ctk.set_default_color_theme("blue")

class LiquidMenu(ctk.CTkToplevel):
    """قائمة رئيسية بتأثير سائل (Liquid UI)"""
    
    def __init__(self, parent, on_launch_overlay):
        super().__init__(parent)
        self.on_launch_overlay = on_launch_overlay
        
        # إعدادات النافذة
        self.title("System Monitor - Main Menu")
        self.geometry("500x400")
        self.resizable(False, False)
        
        # جعل النافذة في المنتصف
        self.center_window()
        
        # إنشاء واجهة المستخدم
        self.create_widgets()
        
    def center_window(self):
        """توسيط النافذة على الشاشة"""
        self.update_idletasks()
        width = 500
        height = 400
        x = (self.winfo_screenwidth() // 2) - (width // 2)
        y = (self.winfo_screenheight() // 2) - (height // 2)
        self.geometry(f'{width}x{height}+{x}+{y}')
    
    def create_widgets(self):
        """إنشاء عناصر الواجهة"""
        # إطار رئيسي بتأثير سائل
        self.main_frame = ctk.CTkFrame(self, corner_radius=30, fg_color="#1a1a2e")
        self.main_frame.pack(fill="both", expand=True, padx=20, pady=20)
        
        # عنوان
        self.title_label = ctk.CTkLabel(
            self.main_frame,
            text="🖥️ System Monitor",
            font=ctk.CTkFont(size=32, weight="bold"),
            text_color="#00d9ff"
        )
        self.title_label.pack(pady=(40, 10))
        
        self.subtitle_label = ctk.CTkLabel(
            self.main_frame,
            text="Monitor your system performance",
            font=ctk.CTkFont(size=16),
            text_color="#a0a0a0"
        )
        self.subtitle_label.pack(pady=(0, 40))
        
        # زر Launch Overlay
        self.launch_btn = ctk.CTkButton(
            self.main_frame,
            text="🚀 Launch Overlay",
            font=ctk.CTkFont(size=18, weight="bold"),
            width=250,
            height=50,
            corner_radius=25,
            fg_color="#00d9ff",
            hover_color="#00b8d9",
            command=self.launch_overlay
        )
        self.launch_btn.pack(pady=20)
        
        # زر الخروج
        self.exit_btn = ctk.CTkButton(
            self.main_frame,
            text="❌ Exit",
            font=ctk.CTkFont(size=16),
            width=250,
            height=45,
            corner_radius=25,
            fg_color="#ff4757",
            hover_color="#ff6b81",
            command=self.quit_app
        )
        self.exit_btn.pack(pady=10)
    
    def launch_overlay(self):
        """إطلاق واجهة العرض الجانبية"""
        self.withdraw()  # إخفاء القائمة الرئيسية
        self.on_launch_overlay()
    
    def quit_app(self):
        """الخروج من التطبيق"""
        self.destroy()


class OverlayWindow(ctk.CTkToplevel):
    """نافذة جانبية تعرض إحصائيات النظام"""
    
    def __init__(self, parent, on_close):
        super().__init__(parent)
        self.on_close = on_close
        self.running = True
        
        # إعدادات النافذة
        self.title("System Stats Overlay")
        self.geometry("350x650+100+100")
        self.resizable(False, False)
        
        # جعل النافذة فوق كل النوافذ
        self.attributes('-topmost', True)
        
        # إنشاء واجهة المستخدم
        self.create_widgets()
        
        # بدء تحديث الإحصائيات
        self.update_stats()
    
    def create_widgets(self):
        """إنشاء عناصر واجهة الإحصائيات"""
        # إطار رئيسي
        self.main_frame = ctk.CTkFrame(self, corner_radius=20, fg_color="#16213e")
        self.main_frame.pack(fill="both", expand=True, padx=15, pady=15)
        
        # عنوان
        self.header_label = ctk.CTkLabel(
            self.main_frame,
            text="📊 Live Stats",
            font=ctk.CTkFont(size=24, weight="bold"),
            text_color="#00d9ff"
        )
        self.header_label.pack(pady=(20, 15))
        
        # CPU Stats
        self.cpu_frame = self.create_stat_box("🔹 CPU Usage", "#e94560")
        self.cpu_frame.pack(fill="x", padx=15, pady=8)
        
        self.cpu_label = ctk.CTkLabel(
            self.cpu_frame,
            text="Loading...",
            font=ctk.CTkFont(size=16),
            text_color="#ffffff"
        )
        self.cpu_label.pack(pady=10)
        
        # GPU Stats (ملاحظة: pywin32 مطلوب لـ NVIDIA GPU)
        self.gpu_frame = self.create_stat_box("🎮 GPU Usage", "#0f3460")
        self.gpu_frame.pack(fill="x", padx=15, pady=8)
        
        self.gpu_label = ctk.CTkLabel(
            self.gpu_frame,
            text="N/A (Requires NVIDIA GPU)",
            font=ctk.CTkFont(size=14),
            text_color="#a0a0a0"
        )
        self.gpu_label.pack(pady=10)
        
        # RAM Stats
        self.ram_frame = self.create_stat_box("💾 RAM Usage", "#533483")
        self.ram_frame.pack(fill="x", padx=15, pady=8)
        
        self.ram_label = ctk.CTkLabel(
            self.ram_frame,
            text="Loading...",
            font=ctk.CTkFont(size=16),
            text_color="#ffffff"
        )
        self.ram_label.pack(pady=10)
        
        # FPS Counter
        self.fps_frame = self.create_stat_box("⚡ FPS", "#16a085")
        self.fps_frame.pack(fill="x", padx=15, pady=8)
        
        self.fps_label = ctk.CTkLabel(
            self.fps_frame,
            text="60 FPS",
            font=ctk.CTkFont(size=18, weight="bold"),
            text_color="#00ff88"
        )
        self.fps_label.pack(pady=10)
        
        # Network Speed
        self.net_frame = self.create_stat_box("🌐 Network Speed", "#2980b9")
        self.net_frame.pack(fill="x", padx=15, pady=8)
        
        self.net_label = ctk.CTkLabel(
            self.net_frame,
            text="Loading...",
            font=ctk.CTkFont(size=16),
            text_color="#ffffff"
        )
        self.net_label.pack(pady=10)
        
        # زر العودة
        self.back_btn = ctk.CTkButton(
            self.main_frame,
            text="⬅️ Back to Menu",
            font=ctk.CTkFont(size=16, weight="bold"),
            width=200,
            height=40,
            corner_radius=20,
            fg_color="#00d9ff",
            hover_color="#00b8d9",
            command=self.go_back
        )
        self.back_btn.pack(pady=20)
    
    def create_stat_box(self, title, color):
        """إنشاء صندوق إحصائية"""
        frame = ctk.CTkFrame(self.main_frame, corner_radius=15, fg_color=color)
        
        title_label = ctk.CTkLabel(
            frame,
            text=title,
            font=ctk.CTkFont(size=14, weight="bold"),
            text_color="#ffffff"
        )
        title_label.pack(pady=(8, 0))
        
        return frame
    
    def update_stats(self):
        """تحديث الإحصائيات بشكل دوري"""
        if not self.running:
            return
        
        try:
            # CPU
            cpu_percent = psutil.cpu_percent(interval=0.5)
            self.cpu_label.configure(text=f"{cpu_percent}%")
            
            # RAM
            ram = psutil.virtual_memory()
            ram_percent = ram.percent
            ram_used = round(ram.used / (1024 ** 3), 2)
            ram_total = round(ram.total / (1024 ** 3), 2)
            self.ram_label.configure(text=f"{ram_used} GB / {ram_total} GB ({ram_percent}%)")
            
            # Network
            net_io = psutil.net_io_counters()
            # حساب السرعة (مبسط)
            upload_speed = round(net_io.bytes_sent / (1024 * 1024), 2)
            download_speed = round(net_io.bytes_recv / (1024 * 1024), 2)
            self.net_label.configure(
                text=f"⬆️ {upload_speed} MB | ⬇️ {download_speed} MB"
            )
            
            # FPS (ثابت للعرض - يمكن ربطه بتطبيق فعلي)
            # ملاحظة: قياس FPS الفعلي يتطلب hook على تطبيق معين
            current_fps = 60  # قيمة افتراضية
            self.fps_label.configure(text=f"{current_fps} FPS")
            
            # GPU (ملاحظة: يحتاج إلى مكتبات إضافية مثل pynvml لـ NVIDIA)
            # حالياً نعرض رسالة توضيحية
            try:
                import pynvml
                pynvml.nvmlInit()
                handle = pynvml.nvmlDeviceGetHandleByIndex(0)
                gpu_util = pynvml.nvmlDeviceGetUtilizationRates(handle).gpu
                self.gpu_label.configure(text=f"{gpu_util}%")
            except:
                self.gpu_label.configure(text="N/A (No NVIDIA GPU)")
            
        except Exception as e:
            print(f"Error updating stats: {e}")
        
        # تحديث كل ثانية
        self.after(1000, self.update_stats)
    
    def go_back(self):
        """العودة للقائمة الرئيسية"""
        self.running = False
        self.destroy()
        self.on_close()


class SystemMonitorApp(ctk.CTk):
    """التطبيق الرئيسي"""
    
    def __init__(self):
        super().__init__()
        
        self.title("System Monitor")
        self.geometry("800x600")
        self.resizable(False, False)
        
        # إخفاء النافذة الرئيسية (نستخدم Toplevel بدلاً منها)
        self.withdraw()
        
        # عرض القائمة الرئيسية
        self.show_main_menu()
    
    def show_main_menu(self):
        """عرض القائمة الرئيسية"""
        self.main_menu = LiquidMenu(self, self.show_overlay)
    
    def show_overlay(self):
        """عرض واجهة الإحصائيات"""
        self.overlay = OverlayWindow(self, self.show_main_menu)
    
    def run(self):
        """تشغيل التطبيق"""
        self.mainloop()


if __name__ == "__main__":
    app = SystemMonitorApp()
    app.run()
