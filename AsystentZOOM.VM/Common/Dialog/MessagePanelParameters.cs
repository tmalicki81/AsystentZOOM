using AsystentZOOM.VM.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AsystentZOOM.VM.Common.Dialog
{
    public class MsgBoxButtonVM : BaseVM
    {
        [Parent(typeof(IMsgBoxVM))]
        public IMsgBoxVM MsgBox { get; set; }

        private object _keyObj;
        public object KeyObj
        {
            get => _keyObj;
            set => SetValue(ref _keyObj, value, nameof(KeyObj));
        }

        private ImageEnum _icon;
        public ImageEnum Icon
        {
            get => _icon;
            set => SetValue(ref _icon, value, nameof(Icon));
        }

        private object _name;
        public object Name
        {
            get => _name;
            set => SetValue(ref _name, value, nameof(Name));
        }

        private ICommand _setResultCommand;
        public ICommand SetResultCommand
            => _setResultCommand ??= new RelayCommand(SetResult);

        private void SetResult()
        {
            MsgBox.ResultObj = KeyObj;
            MsgBox.ToClose = true;
        }
    }

    public class MsgBoxButtonVM<T> : MsgBoxButtonVM
    {
        public MsgBoxButtonVM() { }

        public MsgBoxButtonVM(T key, string name, ImageEnum icon) :this()
        {
            Key = key;
            Name = name;
            Icon = icon;
        }

        public T Key 
        {
            get => (T)KeyObj;
            set => KeyObj = value;
        }
    }

    public interface IMsgBoxVM
    {
        bool ToClose { get; set; }
        object ResultObj { get; set; }
    }

    public abstract class MsgBoxVM : BaseVM, IMsgBoxVM
    {
        public MsgBoxVM(
            string messageBoxText, string caption, ImageEnum icon,
            object defaultButton, IEnumerable<MsgBoxButtonVM> buttons)
        {
            MessageBoxText = messageBoxText;
            Caption = caption;
            Icon = icon;
            DefaultButtonObj = defaultButton;
            ButtonsObj = new ObservableCollection<MsgBoxButtonVM>(buttons);
        }

        private string _messageBoxText;
        public string MessageBoxText
        {
            get => _messageBoxText;
            set => SetValue(ref _messageBoxText, value, nameof(MessageBoxText));
        }

        private string _caption;
        public string Caption
        {
            get => _caption;
            set => SetValue(ref _caption, value, nameof(Caption));
        }

        private ImageEnum _icon;
        public ImageEnum Icon
        {
            get => _icon;
            set => SetValue(ref _icon, value, nameof(Icon));
        }

        private bool _toClose;
        public bool ToClose
        {
            get => _toClose;
            set => SetValue(ref _toClose, value, nameof(ToClose));
        }

        private object _defaultButtonObj;
        public object DefaultButtonObj
        {
            get => _defaultButtonObj;
            set => SetValue(ref _defaultButtonObj, value, nameof(DefaultButtonObj));
        }

        private ObservableCollection<MsgBoxButtonVM> _buttonsObj;
        public ObservableCollection<MsgBoxButtonVM> ButtonsObj
        {
            get => _buttonsObj;
            set => SetValue(ref _buttonsObj, value, nameof(ButtonsObj));
        }

        private object _resultObj;
        public object ResultObj
        {
            get => _resultObj;
            set => SetValue(ref _resultObj, value, nameof(ResultObj));
        }
    }

    public class MsgBoxVM<T> : MsgBoxVM
    {
        public MsgBoxVM(
            string messageBoxText, string caption, ImageEnum icon,
            T defaultButton, IEnumerable<MsgBoxButtonVM<T>> buttons)

            : base(messageBoxText, caption, icon, defaultButton, buttons)
        {
        }

        public new T DefaultButton
        {
            get => (T)base.DefaultButtonObj;
            set => base.DefaultButtonObj = value;
        }

        public new T Result
        {
            get => (T)base.ResultObj;
            set => base.ResultObj = value;
        }
    }
}
